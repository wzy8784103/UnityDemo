using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

/// <summary>
/// ab加载逻辑
/// 这里有一点需要说明，加载任何东西都是允许为空的，只是输出错误信息，并且为空也会进行计数，这样处理可以省去很多麻烦
/// 比如加载时发现一个依赖是空，如果进行打断，那还需要把之前的依赖计数清空，并且外部还需要判断不为空才可以归入自动卸载基类
/// </summary>
public class AssetBundleLoad : AssetLoadBase<AssetBundleLoad>
{
    public static string FolderName = "Res";
    public static string ZipName = "data1.zip";

    private Dictionary<string, BundleInfo> bundleDic = new Dictionary<string, BundleInfo>();
   
    #region 加载Manifest
    private AssetBundleManifest manifest;
    public AssetBundleManifest Manifest
    {
		get
		{
            if(manifest == null)
			{
                LoadManifest();
            }
            return manifest;

        }
    }
    public void LoadManifest()
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(GetAbPath() + FolderName);
        manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        assetBundle.Unload(false);
    }
    #endregion

    #region 同步加载相关
    public override T LoadAsset<T>(string path)
    {
        //已加载过
        if (AssetDic.ContainsKey(path))
        {
            AssetDic[path].Retain();
            return AssetDic[path].obj as T;
        }
        string abName = GetAbName(path);
        if (!IsLoadBundle(abName))
        {
            //依赖
            string[] depends = Manifest.GetAllDependencies(abName);
            for (int i = 0; i < depends.Length; i++)
            {
                string dependName = depends[i];
                if (!IsLoadBundle(dependName))
                {
                    BundleInfo.CreateInfo(dependName, LoadAssetBundle(dependName));
                }
                else
                {
                    bundleDic[dependName].Retain();
                }
            }
            BundleInfo.CreateInfo(abName, LoadAssetBundle(abName));
        }
        else
        {
            bundleDic[abName].Retain();
        }
        T t = null;
        if (bundleDic[abName].bundle != null)
		{
            t = bundleDic[abName].bundle.LoadAsset<T>(path);
		}
        AssetInfo.CreateInfo(path, t);
        return t;
    }

    /// <summary>
    /// 加载ab，这里就算为Null，也只是输出错误信息，打断加载会导致计数问题变得复杂
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string abName)
	{
        AssetBundle assetBundle = AssetBundle.LoadFromFile(GetLoadPath(abName));
        if (assetBundle == null)
        {
            Debug.LogError("assetBundle==" + assetBundle + "为空");
        }
        return assetBundle;
    }
	#endregion

	#region 异步加载相关
	public override void LoadAssetAsync<T>(string path, Action<AssetLoadInfo> action, object arg = null)
	{
       // Debug.Log("LoadAssetAsync");
        ScriptBridge.Singleton.StartCoroutine(LoadAssetCoroutine<T>(path, action, arg));
	}

	IEnumerator LoadAssetCoroutine<T>(string path, Action<AssetLoadInfo> action, object arg) where T : Object
    {
        yield return LoadAssetCoroutineHelper<T>(path);
        if (action != null)
        {
            action.Invoke(AssetLoadInfo.Create(path, arg));
        }
    }

    public override void LoadAssetAsync<T>(List<string> pathList, Action<List<AssetLoadInfo>> action, object arg = null)
    {
        ScriptBridge.Singleton.StartCoroutine(LoadAssetCoroutine<T>(pathList, action, arg));
    }

    IEnumerator LoadAssetCoroutine<T>(List<string> pathList, Action<List<AssetLoadInfo>> action, object arg) where T : Object
    {
        //foreach (var item in pathList)
        //{
        //    AddPrepareLoadDic(item);
        //}
        foreach (var item in pathList)
        {
            yield return LoadAssetCoroutineHelper<T>(item);
        }
        if (action != null)
        {
            List<AssetLoadInfo> list = new List<AssetLoadInfo>();
            for(int i = 0; i < pathList.Count; i++)
            {
                list.Add(AssetLoadInfo.Create(pathList[i], arg));
            }
            action.Invoke(list);
        }
    }
    IEnumerator LoadAssetCoroutineHelper<T>(string path) where T : Object
    {
        //Debug.Log("LoadAssetCoroutineHelper begin");
        if (IsLoadAsset(path))
		{
            //如果重复对同样的资源进行异步加载，需要等待
            if(prepareLoadDic.ContainsKey(path))
			{
                while (prepareLoadDic.ContainsKey(path))
                {
                    yield return null;
                }
                if (AssetDic.ContainsKey(path))
				{
                    AssetDic[path].Retain();
                    yield break;
                }
                //这里有个极特殊情况，最开始没考虑到，同时异步加载两个资源时，目前的写法是加载第二个资源时进行等待
                //但是有可能第一个资源加载完之后立刻卸载了，导致第二个资源等待完之后全都没了，所以需要触发重新加载
				else
				{
                    yield return LoadAssetCoroutineHelper<T>(path);
                }
            }
			else
			{
                AssetDic[path].Retain();
                yield break;
            }
        }
        AddPrepareLoadDic(path);
        //先添加进容器
        AssetInfo.CreateInfo(path, null);
        string abName = GetAbName(path);
        if (!IsLoadBundle(abName))
        {
            //先添加进容器，防止重复调用
            BundleInfo.CreateInfo(abName, null);
            string[] depends = Manifest.GetAllDependencies(abName);
            for (int i = 0; i < depends.Length; i++)
            {
                string dependName = depends[i];
                if (IsLoadBundle(dependName))
				{
                    //Debug.Log("LoadAssetCoroutineHelper IsLoadBundle=" + dependName);
                    bundleDic[dependName].Retain();
                    continue;
                }
                //先添加进容器，防止重复调用
                BundleInfo.CreateInfo(dependName, null);
                //Debug.Log("LoadAssetCoroutineHelper dependName=" + dependName);
                var dependRequest = AssetBundle.LoadFromFileAsync(GetLoadPath(dependName));
                yield return dependRequest;
                bundleDic[dependName].bundle = dependRequest.assetBundle;
            }
            //Debug.Log("LoadAssetCoroutineHelper abName=" + abName);
            var bundleRequest = AssetBundle.LoadFromFileAsync(GetLoadPath(abName));
            yield return bundleRequest;
            bundleDic[abName].bundle = bundleRequest.assetBundle;
        }
        else
        {
            //Debug.Log("LoadAssetCoroutineHelper IsLoadAbName=" + abName);
            bundleDic[abName].Retain();
        }
        AssetBundle bundle = bundleDic[abName].bundle;
        //Debug.Log("LoadAssetCoroutineHelper path=" + path);
        AssetBundleRequest request = bundle.LoadAssetAsync<T>(path);
        yield return request;
        AssetInfo.SetObject(path, request.asset);
        //Debug.Log("LoadAssetCoroutineHelper over");
        //移除加载中列表
        RemovePrepareLoadDic(path);
        //在异步加载时进来一个卸载，都加载完后在这里统一删除
        if(unLoadCacheDic.ContainsKey(path))
		{
            Debug.Log("unLoadCacheDic=" + path);
            RemoveUnLoadCacheDic(path);
            UnLoadAsset(path);
		}
    }
    #endregion

    #region 场景加载相关(只支持单场景)
    private string lastSceneName = "";
    public override void LoadSceneAsync(string path, Action<AssetLoadInfo> action)
    {
        ScriptBridge.Singleton.StartCoroutine(LoadSceneCoroutine(path, action));
    }

    IEnumerator LoadSceneCoroutine(string path, Action<AssetLoadInfo> action)
    {
        string abName = GetAbName(path);
        if (!IsLoadBundle(abName))
        {
            //先添加进容器
            BundleInfo.CreateInfo(abName, null);
            //加载Manifest 
            string[] depends = Manifest.GetAllDependencies(abName);
            //加载所有的依赖文件
            for (int i = 0; i < depends.Length; i++)
            {
                string dependName = depends[i];
                if (IsLoadBundle(dependName))
                {
                    bundleDic[dependName].Retain();
                    continue;
                }
                //先添加进容器
                BundleInfo.CreateInfo(dependName, null);
                var dependRequest = AssetBundle.LoadFromFileAsync(GetLoadPath(dependName));
                yield return dependRequest;
                bundleDic[dependName].bundle = dependRequest.assetBundle;
            }
            var bundleRequest = AssetBundle.LoadFromFileAsync(GetLoadPath(abName));
            yield return bundleRequest;
            bundleDic[abName].bundle = bundleRequest.assetBundle;
        }
        else
        {
            bundleDic[abName].Retain();
        }
        //加载场景
        yield return SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
        if (lastSceneName != "")
        {
            UnLoadScene(lastSceneName);
        }
        lastSceneName = abName;
        if (action != null)
        {
            AssetLoadInfo info = new AssetLoadInfo();
            info.path = path;
            action.Invoke(info);
        }
    }
#endregion

    #region 卸载相关
    private void UnLoadScene(string abName)
    {
        //卸载场景的时候要把集合给清掉，否则会造成集合中有引用卸载不掉的情况
        string[] depends = Manifest.GetAllDependencies(abName);
        //加载所有的依赖文件;  
        for (int i = 0; i < depends.Length; i++)
        {
            string dependName = depends[i];
            bundleDic[dependName].Release();
        }
        ReleaseClearList();
	}

    public override void UnLoadAsset(string path)
    {
        if(prepareLoadDic.ContainsKey(path))
		{
            AddUnLoadCacheDic(path);
            return;
        }
        AssetDic[path].Release();
        if(AssetInfo.GetRes<Object>(path) == null)
		{
            string abName = GetAbName(path);
            bundleDic[abName].Release();
        }
    }
#endregion

    #region 内部一些方法
	private bool IsLoadBundle(string abName)
	{
        //这里不用加是否为空的判断
		if (!bundleDic.ContainsKey(abName))// || bundleDic[abName].bundle == null)
		{
			return false;
		}
		return true;
	}

    public string GetLoadPath(string name)
    {
        return GetAbPath() + GetAbName(name);
    }

    public static string GetAbParentPath()
    {
        if (GameConfig.debugAssetBundle
            || Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.OSXEditor)
        {
            return Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/AssetBundleData/";
        }
        else
        {
            return Application.persistentDataPath + "/";
        }
    }
    public static string GetAbPath()
    {
        return GetAbParentPath() + FolderName + "/";
    }

    public static string GetAbName(string name)
    {
        return (name.Split('.')[0]).ToLower() + ".ab";
    }

    //根据ab名获取场景名
    public string GetSceneName(string abName)
    {
        string[] sArray = abName.Split(new char[2] { '/', '.' });
        return sArray[sArray.Length - 2];
    }
    #endregion

    public class BundleInfo
    {
        public AssetBundle bundle;

        private string abName;
        private int count = 0;
        public void Retain()
        {
            count++;
        }
        public void Release(bool isRecurse = true)
        {
            count--;
            if (count <= 0)
            {
                if(isRecurse)
				{
                    string[] depends = Singleton.Manifest.GetAllDependencies(abName);
                    for (int i = 0; i < depends.Length; i++)
                    {
                        string dependName = depends[i];
                        //Debug.Log(dependName);
                        Singleton.bundleDic[dependName].Release(false);
                    }
                }
                if (bundle != null)
				{
                    bundle.Unload(true);
                }
                Singleton.bundleDic.Remove(abName);
            }
        }
        public static BundleInfo CreateInfo(string name, AssetBundle bundle)
        {
            AssetBundleLoad ab = Singleton as AssetBundleLoad;

            BundleInfo info = new BundleInfo();
            info.abName = name;
            info.bundle = bundle;
            info.Retain();
            ab.bundleDic.Add(name, info);
            return info;
        }
    }
}

