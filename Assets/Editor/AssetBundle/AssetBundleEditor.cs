using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System.Reflection;

public class AssetBundleEditor
{
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

	#region 打包AssetBundle
	//所有需要独立ab的资源集合
	private static HashSet<string> singlePathSet = new HashSet<string>();
	//用来描述父子节点的集合
	private static Dictionary<string, string> bundleDic = new Dictionary<string, string>();
	public static void BuildAssetBundle()
	{
		singlePathSet.Clear();
		bundleDic.Clear();
		//查找所有一级资源(以及需要被代码直接加载的资源)
		//这里建议所有需要单独加载的资源放在统一的文件夹内，要不容易落下
		//这个模块没有放在外部，而写在代码里，主要是考虑很多路径是变量，放在外部不好管理
		string[] strings = AssetDatabase.FindAssets("t:prefab t:scene t:AudioClip", new string[] { "Assets" });
		for (int i = 0, count = strings.Length; i < count; i++)
		{
			singlePathSet.Add(AssetDatabase.GUIDToAssetPath(strings[i]));
		}
		//所有数据打包
		strings = AssetDatabase.FindAssets("", new string[] { "Assets/Resources/Data" });
		for (int i = 0, count = strings.Length; i < count; i++)
		{
			singlePathSet.Add(AssetDatabase.GUIDToAssetPath(strings[i]));
		}
		//反射资源路径类(因为在这个类里的一定都是需要单独加载的)，遍历所有路径进行添加，由于set添加重复元素不会报错，所以这里不用处理
		FieldInfo[] fieldInfos = typeof(ResPath).GetFields();
		for (int i = 0; i < fieldInfos.Length; i++)
		{
			singlePathSet.Add(fieldInfos[i].GetValue(null).ToString());
		}
		//在这里添加单独引用的资源

		foreach (var item in singlePathSet)
		{
			bundleDic.Add(item, item);
		}
		foreach (var item in singlePathSet)
		{
			CheckDepends(item, item);
		}
		//转为打包格式
		Dictionary<string, List<string>> dependsDic = new Dictionary<string, List<string>>();
		foreach (var kv in bundleDic)
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
		string newHashPath = GetNewHashPath();
		if (File.Exists(newHashPath))
		{
			FileTools.Copy(newHashPath, GetOldHashPath());
		}
		//写入最新hash
		FileTools.WriteAllText(GetNewHashPath(), JsonMapper.ToJson(GetAllAssetbundleHashList()));
		Debug.Log("打包完成");
	}
	public static void DeleteUnUsefulAssetBundle()
	{
		string manifestPath = AssetBundleLoad.GetAbPath() + AssetBundleLoad.FolderName;
		AssetBundle assetBundle = AssetBundle.LoadFromFile(manifestPath);
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
	public static void CheckDepends(string rootPath, string path)
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
	public static void ChangeDepends(string rootPath, string path)
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

	public static bool IsSingle(string path)
	{
		return bundleDic.ContainsKey(path) && bundleDic[path] == path;
	}
	#endregion

	#region 压缩
	/// <summary>
	/// 全部压缩
	/// </summary>
	public static void CompressAll()
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
	public static void CompressDiff()
	{
		if (!File.Exists(GetOldHashPath()))
		{
			Debug.LogError("无法找到上次打包hash文件，无法差异压缩，已转为全部压缩");
			CompressAll();
			return;
		}
		//压缩差异资源
		List<string> diffList = GetDiffHashList(GetOldHashPath(), GetNewHashPath());
		string command = "a -tzip " + AssetBundleLoad.ZipName + " -aoa";
		foreach (var item in diffList)
		{
			command += " ";
			//记得用引号啊，被坑了，有的美术资源带空格
			command += ("\"" + item + "\"");
		}
		EditorTools.RunCommand("7z", command, AssetBundleLoad.GetAbParentPath());

		//从压缩包中删除已经没有的文件
		command = "d " + AssetBundleLoad.ZipName;
		List<string> deleteList = GetZipDeleteList(GetOldHashPath(), GetNewHashPath());
		foreach (var item in deleteList)
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
	public static void CompressForHotfix(bool isUpdateHash)
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
	public static string GetNewHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + newHashName;
	}
	public static string GetOldHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + oldHashName;
	}
	public static string GetRemoteHashPath()
	{
		return AssetBundleLoad.GetAbParentPath() + remoteHashName;
	}
	public static string GetHotfixZipPath()
	{
		return AssetBundleLoad.GetAbParentPath() + hotfixZipName;
	}
	public static string GetVersionPath()
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
	public static List<string> GetDiffHashList(string oldPath, string newPath)
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
			result.Add(AssetBundleLoad.FolderName + "/" + info.abName);
		}
		return result;
	}

	/// <summary>
	/// 查找在新hash中没有的文件，需要删除
	/// </summary>
	/// <param name="oldPath"></param>
	/// <returns></returns>
	public static List<string> GetZipDeleteList(string oldPath, string newPath)
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
				result.Add(AssetBundleLoad.FolderName + "/" + info.abName);
				Debug.Log("删除文件==" + info.abName);
			}
		}
		return result;
	}
	#endregion

	#region SFTP
	public static void DownLoadHash()
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
	public static void UpLoadNewHash()
	{
		if (SFTPHelper.UpLoad(
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
	public static void UpLoadHotfixZip()
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
	public static void ChangeVersion()
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

	/// <summary>
	/// 把所有的shader都加到always include中，要不打assetbundle时候每个用到内置shader都会在自己Bundle中复制一份
	/// 或者也可以把内置shader拷贝到项目里，但是没试过
	/// </summary>
	[MenuItem("Tools/AssetBundle/SetAlwaysIncluderShders")]
	static void SetAlwaysIncluderShders()
	{
		List<string> insideShaderList = new List<string>();
		insideShaderList.Add("Particles/Alpha Blended");
		insideShaderList.Add("Particles/Additive");
		insideShaderList.Add("Mobile/Particles/Additive");
		insideShaderList.Add("Legacy Shaders/Diffuse");
		insideShaderList.Add("Mobile/Particles/Alpha Blended");
		//insideShaderList.Add("Standard");
		insideShaderList.Add("Mobile/Diffuse");
		insideShaderList.Add("Legacy Shaders/Transparent/Diffuse");
		insideShaderList.Add("Legacy Shaders/Self-Illumin/Diffuse");
		insideShaderList.Add("Particles/Additive (Soft)");
		insideShaderList.Add("Unlit/Transparent");
		insideShaderList.Add("Legacy Shaders/Transparent/VertexLit");
		insideShaderList.Add("Legacy Shaders/Reflective/Diffuse");
		insideShaderList.Add("Mobile/Unlit (Supports Lightmap)");
		insideShaderList.Add("Legacy Shaders/Specular");
		insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Diffuse");
		insideShaderList.Add("Particles/Alpha Blended Premultiply");
		insideShaderList.Add("Legacy Shaders/Transparent/Bumped Specular");
		insideShaderList.Add("Unlit/Texture");
		insideShaderList.Add("Skybox/6 Sided");
		insideShaderList.Add("Legacy Shaders/Transparent/Parallax Specular");
		insideShaderList.Add("Legacy Shaders/VertexLit");
		insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Diffuse");
		insideShaderList.Add("Legacy Shaders/Transparent/Bumped Diffuse");
		insideShaderList.Add("Mobile/Bumped Diffuse");
		insideShaderList.Add("Legacy Shaders/Bumped Diffuse");
		insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit");
		insideShaderList.Add("Legacy Shaders/Transparent/Specular");
		insideShaderList.Add("Legacy Shaders/Bumped Specular");
		insideShaderList.Add("Mobile/Bumped Specular");
		//Dictionary<string, bool> assetDic = new Dictionary<string, bool>();
		//Dictionary<string, Shader> shaderDic = new Dictionary<string, Shader>();
		//string[] allAssets = AssetDatabase.GetAllAssetPaths();
		//for (int i = 0, count = allAssets.Length; i < count; i++)
		//{
		//    string dpName = allAssets[i];
		//    if (dpName.EndsWith(".mat"))
		//    {
		//        Material mat = (Material)AssetDatabase.LoadMainAssetAtPath(dpName);
		//        Shader shader = mat.shader;
		//        if (shader.name.Equals("GUI/Text Shader") || shader.name.Equals("Hidden/InternalErrorShader"))
		//        {
		//            continue;
		//        }
		//        if (!shaderDic.ContainsKey(shader.name))
		//        {
		//            shaderDic.Add(shader.name, shader);
		//        }
		//    }
		//}

		SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
		SerializedProperty it = graphicsSettings.GetIterator();
		SerializedProperty dataPoint;
		while (it.NextVisible(true))
		{
			if (it.name == "m_AlwaysIncludedShaders")
			{
				it.ClearArray();
				for (int i = 0; i < insideShaderList.Count; i++)
				{
					it.InsertArrayElementAtIndex(i);
					dataPoint = it.GetArrayElementAtIndex(i);
					dataPoint.objectReferenceValue = Shader.Find(insideShaderList[i]);
				}

				graphicsSettings.ApplyModifiedProperties();
			}
		}

		Debug.Log("添加完成");
	}
}
