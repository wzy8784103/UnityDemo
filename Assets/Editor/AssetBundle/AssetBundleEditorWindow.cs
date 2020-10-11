using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System.Reflection;

/// <summary>
/// 打包UI和逻辑，都写到这了
/// 基本流程：先列出所有需要代码加载的资源，可以指定文件夹或者代码写，目前用的是代码写
/// 放入singlePathSet中，代码会自动以最优粒度处理依赖并打包
/// </summary>
public class AssetBundleEditorWindow : EditorWindow
{
	//0打包 1热更
    private int selectTab;

	//是否压缩
	private bool isCompress = false;
	//是否全部压缩，否的话差异压缩
	private bool isCompressAll = true;
	//是否更新最新hash
	private bool isUpdateHash = true;
	//是否上传最新压缩包
	private bool isUpLoadHotfixZip = true;
	//是否修改线上版本号
	private bool isChangeVersion = true;

	//是否压缩更新包
	private bool isCompressHotfix = true;
	//是否上传最新hash
	private bool isUpLoadHash = false;

	//每次新打ab的最新hash文件
	private const string newHashName = "AssetBundleHashNew.txt";
	//上次ab的hash文件
	private const string oldHashName = "AssetBundleHashOld.txt";
	//服务器的hash文件，用于热更新比较差异
	private const string remoteHashName = "AssetBundleHashRemote.txt";
	//热更压缩包名
	private const string hotfixZipName = "HotfixZip.zip";
	//ip版本文件
	private const string versionName = "version.txt";

	//服务器账号密码
	private const string sftpIp = "*";
	private const string sftpUserName = "*";
	private const string sftpPassword = "*";
	private const string sftpPath = "/home/apache-tomcat-7.0.55/webapps/";

	private void OnGUI()
    {
        selectTab = GUILayout.Toolbar(selectTab, new string[] { "Pack", "Hotfix(需配置7z环境变量)"});
		EditorGUILayout.Space();
		//Pack
		if (selectTab == 0)
        {
			EditorGUILayout.BeginHorizontal();
			isCompress = EditorGUILayout.ToggleLeft("压缩AssetBundle(需下载7z并配置环境变量)", isCompress);
			EditorGUILayout.EndHorizontal();
			if(isCompress)
			{
				EditorGUILayout.BeginHorizontal();
				isCompressAll = EditorGUILayout.ToggleLeft("全部重新压缩", isCompressAll);
				if (GUILayout.Button("单独执行"))
				{
					CompressAll();
				}
				isCompressAll = EditorGUILayout.ToggleLeft("差异压缩", !isCompressAll);
				if (GUILayout.Button("单独执行"))
				{
					CompressDiff();
				}
				EditorGUILayout.EndHorizontal();
			}
		}
        //Hotfix
        else if (selectTab == 1)
        {
			EditorGUILayout.BeginHorizontal();
			isUpdateHash = EditorGUILayout.ToggleLeft("更新线上hash文件", isUpdateHash);
			if (GUILayout.Button("单独执行"))
			{
				DownLoadHash();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isCompressHotfix = EditorGUILayout.ToggleLeft("比较hash差异并压缩(未勾选更新hash将用本地hash)", isCompressHotfix);
			if (GUILayout.Button("单独执行"))
			{
				CompressForHotfix();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isUpLoadHotfixZip = EditorGUILayout.ToggleLeft("上传更新压缩包", isUpLoadHotfixZip);
			if (GUILayout.Button("单独执行"))
			{
				UpLoadHotfixZip();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isChangeVersion = EditorGUILayout.ToggleLeft("更新版本号(最后一位+1)", isChangeVersion);
			if (GUILayout.Button("单独执行"))
			{
				ChangeVersion();
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		isUpLoadHash = EditorGUILayout.ToggleLeft("上传hash文件", isUpLoadHash);
		if (GUILayout.Button("单独执行"))
		{
			UpLoadNewHash();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		if (GUILayout.Button("开始执行"))
        {
			BuildAssetBundle();
			if(selectTab == 0)
			{
				if (isCompress)
				{
					if (isCompressAll)
					{
						CompressAll();
					}
					else
					{
						CompressDiff();
					}
				}
			}
			else if (selectTab == 1)
			{
				if (isUpdateHash) DownLoadHash();
				if (isCompressHotfix) CompressForHotfix();
				if (isUpLoadHotfixZip) UpLoadHotfixZip();
				if (isChangeVersion) ChangeVersion();
			}
			if (isUpLoadHash)
			{
				UpLoadNewHash();
			} 
        }
    }

	#region 打包AssetBundle
	//所有需要独立ab的资源集合
	private HashSet<string> singlePathSet = new HashSet<string>();
	//用来描述父子节点的集合
    private Dictionary<string, string> bundleDic = new Dictionary<string, string>();
    public void BuildAssetBundle()
    {
        DateTime t1 = DateTime.Now;
        singlePathSet.Clear();
        bundleDic.Clear();
		//查找所有一级资源(以及需要被代码直接加载的资源)
		//这里建议所有需要单独加载的资源放在统一的文件夹内，要不容易落下
		//这个模块没有放在外部，而写在代码里，主要是考虑很多路径是变量，放在外部不好管理
		string[] strings1 = AssetDatabase.FindAssets("t:prefab t:scene t:AudioClip", new string[] { "Assets" });
		for (int i = 0, count = strings1.Length; i < count; i++)
		{
			singlePathSet.Add(AssetDatabase.GUIDToAssetPath(strings1[i]));
		}

		//singlePathSet.Add("Assets/Resources/Canvas 1.prefab");
		//singlePathSet.Add("Assets/Resources/Canvas 2.prefab");
		//singlePathSet.Add("Assets/Resources/New Sprite Atlas.spriteatlas");
		//singlePathSet.Add("Assets/Resources/Test/tex_other_02.png");

		//反射资源路径类(因为在这个类里的一定都是需要单独加载的)，遍历所有路径进行添加，由于set添加重复元素不会报错，所以这里不用处理
		FieldInfo[] fieldInfos = typeof(ResPath).GetFields();
		for (int i = 0; i < fieldInfos.Length; i++)
		{
			singlePathSet.Add(fieldInfos[i].GetValue(null).ToString());
		}

		//在这里添加单独引用的资源

		DateTime t2 = DateTime.Now;
        Debug.Log(t2 - t1);
        foreach (var item in singlePathSet)
		{
            bundleDic.Add(item, item);
		}
        foreach (var item in singlePathSet)
		{
            CheckDepends(item, item);
        }

        DateTime t3 = DateTime.Now;
        Debug.Log(t3 - t2);
        //转为打包格式
        Dictionary<string, List<string>> dependsDic = new Dictionary<string, List<string>>();
        foreach(var kv in bundleDic)
		{
            string singlePath = kv.Value;
			//场景特殊处理，场景不能和其他东西在一个Bundle里
			if (singlePath.EndsWith(".unity"))
			{
				singlePath = kv.Key;
			}
			if (!dependsDic.ContainsKey(singlePath))
			{
                dependsDic.Add(singlePath, new List<string>());
            }
            dependsDic[singlePath].Add(kv.Key);
        }

        //最终打包List
        List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
        foreach (var kv in dependsDic)
        {
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = AssetBundleLoad.GetAbName(kv.Key);
            build.assetNames = kv.Value.ToArray();
            buildList.Add(build);
        }

        Debug.Log("buildList===" + buildList.Count);
        string path = AssetBundleLoad.GetAbPath();
        //这里得创建下文件夹，他不会帮你创建
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, buildList.ToArray(), BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        //删除多余资源
        DeleteUnUsefulAssetBundle();

		//把新hash变成老的
		FileTools.Copy(GetNewHashPath(), GetOldHashPath());
		//写入最新hash
		FileTools.WriteAllText(GetNewHashPath(), JsonMapper.ToJson(GetAllAssetbundleHashList()));
		Debug.Log(DateTime.Now - t3);
        Debug.Log("打包完成");
    }

	private void DeleteUnUsefulAssetBundle()
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetBundleLoad.GetAbPath() + AssetBundleLoad.FolderName);
        AssetBundleManifest manifest = (AssetBundleManifest)assetBundle.LoadAsset("AssetBundleManifest", typeof(AssetBundleManifest));
        string[] allAssetbundleNames = manifest.GetAllAssetBundles();
        HashSet<string> set = new HashSet<string>();
        for (int i = 0, count = allAssetbundleNames.Length; i < count; i++)
        {
            set.Add(allAssetbundleNames[i]);
        }
        assetBundle.Unload(false);
        DeleteAssetBundle(AssetBundleLoad.GetAbPath(), set);
    }

    static void DeleteAssetBundle(string path, HashSet<string> set)
    {
        DirectoryInfo folder = new DirectoryInfo(path);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                DeleteAssetBundle(files[i].FullName, set);
                continue;
            }
            if (files[i].Name.EndsWith(".assetbundle"))
            {
                string fullName = files[i].FullName.Replace("\\", "/");
                string newName = fullName.Substring(path.Length, fullName.Length - path.Length);
                if (!set.Contains(newName))
                {
                    //manifest文件也一起删掉
                    System.IO.File.Delete(fullName + ".manifest");
                    files[i].Delete();
                    Debug.Log("已删除" + files[i].FullName);
                }
            }
        }
    }

    private void CheckDepends(string rootPath, string path)
    {
        //获取依赖,false为只考虑这个资源的依赖，不考虑依赖的依赖
        string[] depends = AssetDatabase.GetDependencies(path, false);
		if (depends.Length == 0)
		{
			return;
		}

		for (int i = 0; i < depends.Length; i++)
        {
            string depend = depends[i];
			//排除代码
			if (depend.EndsWith(".cs") || depend.EndsWith(".js"))
			{
                continue;
            }

			if (bundleDic.ContainsKey(depend))
			{
				//如果被不同的东西依赖，则单独打包
				if (!IsSingle(depend) && bundleDic[depend] != rootPath)
				{
					//把所有依赖都改为自己的子树
					bundleDic[depend] = depend;
					ChangeDepends(depend, depend);
				}
			}
			else
			{

				bundleDic.Add(depend, rootPath);
				CheckDepends(rootPath, depend);
			}
		}
    }

    private void ChangeDepends(string rootPath, string path)
	{
        string[] allDepends = AssetDatabase.GetDependencies(path, false);
		foreach (var item in allDepends)
		{
			if (item.EndsWith(".cs") || item.EndsWith(".js"))
			{
				continue;
			}
			if (!IsSingle(item))
			{
				bundleDic[item] = rootPath;
                ChangeDepends(rootPath, item);
            }
			else
			{
                ChangeDepends(item, item);
            }
        }
	}

    private bool IsSingle(string path)
	{
        return bundleDic.ContainsKey(path) && bundleDic[path] == path;
    }
	#endregion

	#region 压缩
	/// <summary>
	/// 全部压缩
	/// </summary>
	private void CompressAll()
	{
		string command =
			"a -tzip " + AssetBundleLoad.ZipName + " -aoa "
			+ AssetBundleLoad.FolderName + "/" + AssetBundleLoad.FolderName + " "
			+ AssetBundleLoad.FolderName + "/*.ab -r";
		//EditorTools.RunCommand(@"D:\7z\7-Zip\7z.exe", command, AssetBundleLoad.GetAbParentPath(), true);
		EditorTools.RunCommand("7z", command, AssetBundleLoad.GetAbParentPath(), true);
	}
	/// <summary>
	/// 压缩修改部分
	/// </summary>
	private void CompressDiff()
	{
		if(!File.Exists(GetOldHashPath()))
		{
			Debug.LogError("无法找到上次打包hash文件，无法差异压缩，已转为全部压缩");
			CompressAll();
			return;
		}
		//压缩差异资源
		List<string> diffList = GetDiffHashList(GetOldHashPath(), GetNewHashPath());
		string command = "a -tzip " + AssetBundleLoad.ZipName + " -aoa";
		foreach(var item in diffList)
		{
			command += " ";
			//记得用引号啊，被坑了，有的美术资源带空格
			command += ("\"" + item + "\""); 
		}
		EditorTools.RunCommand("7z", command, AssetBundleLoad.GetAbParentPath());

		//从压缩包中删除已经没有的文件
		command = "d " + AssetBundleLoad.ZipName;
		List<string> deleteList = GetZipDeleteList(GetOldHashPath(), GetNewHashPath());
		foreach(var item in deleteList)
		{
			command += " ";
			command += ("\"" + item + "\"");
		}
		EditorTools.RunCommand("7z", command, AssetBundleLoad.GetAbParentPath());
		Debug.Log("差异压缩完成");
	}
	/// <summary>
	/// 比较hash文件，把差异单独压缩
	/// </summary>
	public void CompressForHotfix()
	{
		List<string> diffList = GetDiffHashList(isUpdateHash ? GetRemoteHashPath() : GetOldHashPath(), GetNewHashPath());
		EditorTools.RunCommand("7z", @"d " + GetHotfixZipPath(), AssetBundleLoad.GetAbParentPath());
		string command = @"a -tzip " + hotfixZipName;
		for (int i = 0; i < diffList.Count; i++)
		{
			command += " ";
			command += ("\"" + diffList[i] + "\"");
		}
		EditorTools.RunCommand("7z", command, AssetBundleLoad.GetAbParentPath());
		Debug.Log("热更压缩完成");
	}
	#endregion

	#region 文件操作相关
	private string GetNewHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + newHashName;
	}
	private string GetOldHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + oldHashName;
	}
	private string GetRemoteHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + remoteHashName;
	}
	private string GetHotfixZipPath()
	{
		return AssetBundleLoad.GetAbParentPath() + hotfixZipName;
	}
	private string GetVersionPath()
	{
		return AssetBundleLoad.GetAbParentPath() + versionName;
	}
	/// <summary>
	/// 拿到所有ab的hash
	/// </summary>
	/// <returns></returns>
	public static List<AssetBundleHashInfo> GetAllAssetbundleHashList()
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetBundleLoad.GetAbPath() + AssetBundleLoad.FolderName);
		AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		List<AssetBundleHashInfo> assetbundleHashInfoList = new List<AssetBundleHashInfo>();
		string[] allAssetbundleNames = manifest.GetAllAssetBundles();
		for (int i = 0; i < allAssetbundleNames.Length; i++)
		{
			string abName = allAssetbundleNames[i];
			AssetBundleHashInfo info = new AssetBundleHashInfo();
			info.abName = abName;
			info.hash = manifest.GetAssetBundleHash(abName).ToString();
			assetbundleHashInfoList.Add(info);
		}
		//Manifest别忘了加
		AssetBundleHashInfo info2 = new AssetBundleHashInfo();
		info2.abName = AssetBundleLoad.FolderName;
		info2.hash = manifest.GetHashCode().ToString();
		assetbundleHashInfoList.Add(info2);
		assetBundle.Unload(true);
		return assetbundleHashInfoList;
	}
	/// <summary>
	/// 查找差异hash
	/// </summary>
	/// <param name="oldPath">旧文件</param>
	/// <param name="newPath">新文件</param>
	/// <returns></returns>
	private List<string> GetDiffHashList(string oldPath, string newPath)
	{
		List<string> result = new List<string>();
		if (!File.Exists(oldPath))
		{
			Debug.LogError(oldPath + "不存在");
			return result;
		}
		if (!File.Exists(newPath))
		{
			Debug.LogError(newPath + "不存在");
			return result;
		}

		Dictionary<string, string> oldDic = new Dictionary<string, string>();
		//比较新旧差异
		List<AssetBundleHashInfo> oldList = JsonMapper.ToObject<List<AssetBundleHashInfo>>(File.ReadAllText(oldPath));
		for (int i = 0; i < oldList.Count; i++)
		{
			AssetBundleHashInfo info = oldList[i];
			oldDic.Add(info.abName, info.hash);
		}

		List<AssetBundleHashInfo> newList = JsonMapper.ToObject<List<AssetBundleHashInfo>>(File.ReadAllText(newPath));
		for (int i = 0; i < newList.Count; i++)
		{
			AssetBundleHashInfo info = newList[i];
			//相同
			if (oldDic.ContainsKey(info.abName) && oldDic[info.abName].Equals(info.hash))
			{
				continue;
			}
			result.Add(info.abName);
		}
		return result;
	}

	/// <summary>
	/// 查找在新hash中没有的文件，需要删除
	/// </summary>
	/// <param name="oldPath"></param>
	/// <returns></returns>
	private List<string> GetZipDeleteList(string oldPath, string newPath)
	{
		List<string> result = new List<string>();
		if (!File.Exists(oldPath))
		{
			Debug.LogError(oldPath + "不存在");
			return result;
		}
		if (!File.Exists(newPath))
		{
			Debug.LogError(newPath + "不存在");
			return result;
		}

		List<AssetBundleHashInfo> newList = JsonMapper.ToObject<List<AssetBundleHashInfo>>(File.ReadAllText(newPath));
		Dictionary<string, string> newDic = new Dictionary<string, string>();
		for (int i = 0, count = newList.Count; i < count; i++)
		{
			AssetBundleHashInfo info = newList[i];
			newDic.Add(info.abName, info.hash);
		}

		//比较新旧差异， 自动找出更新包
		List<AssetBundleHashInfo> oldList = LitJson.JsonMapper.ToObject<List<AssetBundleHashInfo>>(File.ReadAllText(oldPath));
		for (int i = 0; i < oldList.Count; i++)
		{
			AssetBundleHashInfo info = oldList[i];
			if (!newDic.ContainsKey(info.abName))
			{
				result.Add(info.abName);
				Debug.Log("删除文件==" + info.abName);
			}
		}
		return result;
	}
	#endregion

	#region SFTP
	private void DownLoadHash()
	{
		if (SFTPHelper.DownLoad(
			new SFTPHelper(sftpIp, sftpUserName, sftpPassword),
			GetRemoteHashPath(),
			sftpPath + GameConfig.remoteFolder + "/" + newHashName))
		{
			Debug.Log("下载hash成功");
		}
		else
		{
			Debug.LogError("下载hash失败");
		}
	}
	private void UpLoadNewHash()
	{
		if(SFTPHelper.UpLoad(
			new SFTPHelper(sftpIp, sftpUserName, sftpPassword), 
			GetNewHashPath(),
			sftpPath + GameConfig.remoteFolder + "/" + newHashName))
		{
			Debug.Log("上传hash成功");
		}
		else 
		{
			Debug.LogError("上传hash失败");
		}
	}
	private void UpLoadHotfixZip()
	{
		//先更新版本号文件
		if (SFTPHelper.DownLoad(
			new SFTPHelper(sftpIp, sftpUserName, sftpPassword),
			GetVersionPath(),
			sftpPath + GameConfig.remoteFolder + "/" + versionName))
		{
			Debug.Log("下载版本文件成功");
			VersionInfo versionInfo = JsonMapper.ToObject<VersionInfo>(File.ReadAllText(GetVersionPath()));
			
			//重命名压缩包，命名规则为当前版本号最后一位+1
			if (SFTPHelper.UpLoad(
				new SFTPHelper(sftpIp, sftpUserName, sftpPassword),
				GetHotfixZipPath(),
				sftpPath + GameConfig.remoteFolder + "/" + VersionTools.AddAbVersion(versionInfo.version) + ".zip"))
			{
				Debug.Log("上传热更包成功");
			}
			else
			{
				Debug.LogError("上传热更包失败");
			}
		}
		else
		{
			Debug.LogError("下载版本文件失败");
		}
	}
	private void ChangeVersion()
	{
		//先更新版本号文件
		if (SFTPHelper.DownLoad(
			new SFTPHelper(sftpIp, sftpUserName, sftpPassword),
			GetVersionPath(),
			sftpPath + GameConfig.remoteFolder + "/" + versionName))
		{
			Debug.Log("下载版本文件成功");
			//最后一位+1
			VersionInfo versionInfo = JsonMapper.ToObject<VersionInfo>(File.ReadAllText(GetVersionPath()));
			versionInfo.version = VersionTools.AddAbVersion(versionInfo.version);
			//上传最新版本号文件
			if (SFTPHelper.UpLoad(
				new SFTPHelper(sftpIp, sftpUserName, sftpPassword),
				GetVersionPath(),
				sftpPath + GameConfig.remoteFolder + "/" + versionName))
			{
				Debug.Log("上传版本文件成功");
			}
			else
			{
				Debug.LogError("上传版本文件失败");
			}
		}
		else
		{
			Debug.LogError("下载版本文件失败");
		}
	}
	#endregion
	public class AssetBundleHashInfo
    {
        public string abName;
        public string hash;
    }
	public class VersionInfo
	{
		public string version;
		public string ip;
		public string testVersion;
		public string testIp;
	}
}
