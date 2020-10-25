using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

public interface IAssetLoad
{
    T LoadAsset<T>(string fullPath, AutoReleaseBase autoRelease) where T : Object;
    T LoadClearAsset<T>(string fullPath) where T : Object;
    void LoadAssetAsync<T>(string fullPath, Action<AssetLoadInfo> action, AutoReleaseBase autoRelease, object arg = null) where T : Object;
    void LoadAssetAsync<T>(List<string> pathList, Action<List<AssetLoadInfo>> action, AutoReleaseBase autoRelease, object arg = null) where T : Object;
    void LoadSceneAsync(string fullPath, Action<AssetLoadInfo> action);
    void UnLoadAsset(string fullPath);
    void ClearAll();
    void ReleaseClearList();
    Dictionary<string, AssetInfo> AssetDic { get; }
}

/// <summary>
/// 这里这么写看起来挺麻烦的，因为我想让子类都可以独立单例
/// 因为实际开发中会出现某些情况需要特殊处理,比如一些初始的资源加载需要用resource，不能用ab
/// 然后又想所有子类有统一接口去调用，所以这里抽象了一个接口，用于我的目的
/// </summary>
/// <typeparam name="TChild"></typeparam>
public abstract class AssetLoadBase<TChild> : SingletonBase<TChild>, IAssetLoad where TChild : SingletonBase<TChild>, new()
{
    //加载过得资源记录
	private Dictionary<string, AssetInfo> assetDic = new Dictionary<string, AssetInfo>();
    public Dictionary<string, AssetInfo> AssetDic { get { return assetDic; } }

    //所有切场景需要清理的资源记录，或者一些特定清理点
    protected List<string> clearList = new List<string>();
    //正在异步加载中的资源，防止异步加载资源A时又进入一个异步加载资源A的请求
    protected Dictionary<string, int> prepareLoadDic = new Dictionary<string, int>();
    //卸载时可能资源正在异步加载，所以这里缓存一下，等全部加载完之后再调用卸载
    protected Dictionary<string, int> unLoadCacheDic = new Dictionary<string, int>();

    /// <summary>
    /// 同步加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullPath">完整资源路径</param>
    /// <param name="autoRelease">自动卸载基类</param>
    /// <returns></returns>
    public T LoadAsset<T>(string fullPath, AutoReleaseBase autoRelease) where T : Object
    {
        RetainAssets(fullPath, autoRelease);
        return LoadAsset<T>(fullPath);
    }
    /// <summary>
    /// 加载资源，此资源会在切换场景时清理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullPath">完整资源路径</param>
    /// <returns></returns>
    public T LoadClearAsset<T>(string fullPath) where T : Object
    {
        clearList.Add(fullPath);
        return LoadAsset<T>(fullPath);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullPath">完整资源路径</param>
    /// <param name="action">加载完成回调</param>
    /// <param name="arg">携带参数</param>
    /// <param name="autoRelease">自动卸载基类</param>
    public void LoadAssetAsync<T>(string fullPath, Action<AssetLoadInfo> action, AutoReleaseBase autoRelease, object arg = null) where T : Object
    {
        RetainAssets(fullPath, autoRelease);
        LoadAssetAsync<T>(fullPath, action, arg);
    }

    public void LoadAssetAsync<T>(List<string> pathList, Action<List<AssetLoadInfo>> action, AutoReleaseBase autoRelease, object arg = null) where T : Object
    {
        foreach(var item in pathList)
        {
            RetainAssets(item, autoRelease);
        }
        LoadAssetAsync<T>(pathList, action, arg);
    }

    /// <summary>
    /// 加一个清理的方法，用于重连或者注销等操作
    /// </summary>
    public void ClearAll()
    {
        foreach(string path in assetDic.Keys.ToList())
        {
            UnLoadAsset(path);
        }
        assetDic.Clear();
        clearList.Clear();
        Clear();
    }

    protected virtual void Clear()
    {

    }
    private void RetainAssets(string fullPath, AutoReleaseBase autoRelease)
    {
        if (autoRelease != null)
        {
            autoRelease.AssetRetain(fullPath);
        }
    }

    public void ReleaseClearList()
	{
        foreach (var item in clearList)
        {
            UnLoadAsset(item);
        }
        clearList.Clear();
    }

	//是否加载obj
	protected bool IsLoadAsset(string fullPath)
	{
		if (!assetDic.ContainsKey(fullPath))// || assetDic[fullPath].obj == null)
		{
			return false;
		}
		return true;
	}

    public void AddUnLoadCacheDic(string fullPath)
    {
        if (!unLoadCacheDic.ContainsKey(fullPath))
        {
            unLoadCacheDic.Add(fullPath, 0);
        }
        unLoadCacheDic[fullPath]++;
    }
    public void RemoveUnLoadCacheDic(string fullPath)
    {
        unLoadCacheDic[fullPath]--;
        if (unLoadCacheDic[fullPath] == 0)
        {
            unLoadCacheDic.Remove(fullPath);
        }
    }
    public void AddPrepareLoadDic(string fullPath)
    {
        if (!prepareLoadDic.ContainsKey(fullPath))
        {
            prepareLoadDic.Add(fullPath, 0);
        }
        prepareLoadDic[fullPath]++;
    }
    public void RemovePrepareLoadDic(string fullPath)
    {
        prepareLoadDic[fullPath]--;
        if (prepareLoadDic[fullPath] == 0)
        {
            prepareLoadDic.Remove(fullPath);
        }
    }

    public abstract T LoadAsset<T>(string fullPath) where T : Object;
    public abstract void LoadAssetAsync<T>(string fullPath, Action<AssetLoadInfo> action, object arg = null) where T : Object;
    public abstract void LoadAssetAsync<T>(List<string> pathList, Action<List<AssetLoadInfo>> action, object arg = null) where T : Object;
    public abstract void LoadSceneAsync(string fullPath, Action<AssetLoadInfo> action);
    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="fullPath">资源名</param>
    public abstract void UnLoadAsset(string fullPath);

}
public class AssetInfo
{
    public Object obj;

    private string path;
    private int count = 0;
    public void Retain()
    {
        count++;
    }
    public void Release()
    {
        count--;
        if (count <= 0)
        {
            AssetManager.AssetLoad.AssetDic.Remove(path);
        }
    }
    public int GetCount()
	{
        return count;
    }

    public static AssetInfo CreateInfo(string path, Object obj)
    {
        AssetInfo info = new AssetInfo();
        info.path = path;
        info.obj = obj;
        info.Retain();
        Dictionary<string, AssetInfo> assetDic = AssetManager.AssetLoad.AssetDic;
        if (!assetDic.ContainsKey(path))
        {
            assetDic.Add(path, null);
        }
        assetDic[path] = info;
        return info;
    }
    public static void SetObject(string path, Object obj)
    {
        AssetManager.AssetLoad.AssetDic[path].obj = obj;
    }
    public static AssetInfo GetAssetInfo(string path)
    {
        return AssetManager.AssetLoad.AssetDic[path];
    }
    public static T GetRes<T>(string path) where T : Object
    {
        if(!AssetManager.AssetLoad.AssetDic.ContainsKey(path))
		{
            return null;
		}
        return AssetManager.AssetLoad.AssetDic[path].obj as T;
    }
}

public class AssetLoadInfo
{
    public string path;
    public Object obj;
    public object arg;

    public static AssetLoadInfo Create(string path, object arg = null)
    {
        AssetLoadInfo info = new AssetLoadInfo();
        info.path = path;
        info.obj = AssetInfo.GetRes<Object>(path);
        info.arg = arg;
        return info;
    }
}
