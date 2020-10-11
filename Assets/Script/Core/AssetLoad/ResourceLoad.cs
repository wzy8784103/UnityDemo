using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ResourceLoad : AssetLoadBase<ResourceLoad>
{
    #region 同步加载相关
    public override T LoadAsset<T>(string path)
    {
        if(IsLoadAsset(path))
		{
            AssetDic[path].Retain();
            return AssetInfo.GetRes<T>(path);
        }
		T t = Resources.Load<T>(GetResourceLoadPath(path));
        AssetInfo.CreateInfo(path, t);
        return t;
    }
    #endregion

    #region 异步加载相关
    public override void LoadAssetAsync<T>(string path, Action<AssetLoadInfo> action, object arg = null)
    {
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
        foreach (var item in pathList)
        {
            yield return LoadAssetCoroutineHelper<T>(item);
        }
        if (action != null)
        {
            List<AssetLoadInfo> list = new List<AssetLoadInfo>();
            foreach (var item in pathList)
            {
                list.Add(AssetLoadInfo.Create(item));
            }
            action.Invoke(list);
        }
    }

    IEnumerator LoadAssetCoroutineHelper<T>(string path) where T : Object
    {
        if (IsLoadAsset(path))
        {
            if (prepareLoadDic.ContainsKey(path))
            {
                while (prepareLoadDic.ContainsKey(path))
                {
                    //Debug.Log("while(AssetDic[path].obj == null)");
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
            //AssetDic[path].Retain();
            //yield break;
        }
        AddPrepareLoadDic(path);
        AssetInfo.CreateInfo(path, null);
		var request = Resources.LoadAsync<T>(GetResourceLoadPath(path));
		yield return request;
		AssetInfo.SetObject(path, request.asset);
        //移除加载中列表
        RemovePrepareLoadDic(path);
        //在异步加载时进来一个卸载，都加载完后在这里统一删除
        if (unLoadCacheDic.ContainsKey(path))
        {
            RemoveUnLoadCacheDic(path);
        }
    }
    #endregion

    #region 场景加载相关(只支持单场景)
    public override void LoadSceneAsync(string path, Action<AssetLoadInfo> action)
    {
        clearList.Clear();
        ScriptBridge.Singleton.StartCoroutine(LoadSceneCoroutine(path, action));
    }

    IEnumerator LoadSceneCoroutine(string assetName, Action<AssetLoadInfo> action)
    {
        var request = SceneManager.LoadSceneAsync(assetName, LoadSceneMode.Single);
        yield return request;

        if (action != null)
        {
            AssetLoadInfo info = new AssetLoadInfo();
            info.path = assetName;
            action.Invoke(info);
        }
    }
    #endregion

    #region 卸载相关
    public override void UnLoadAsset(string path)
    {
        if (prepareLoadDic.ContainsKey(path))
        {
            AddUnLoadCacheDic(path);
            return;
        }
        AssetDic[path].Release();
    }
#endregion

    private string GetResourceLoadPath(string path)
    {
        string pathName = "Assets/Resources/";
        return path.Substring(pathName.Length, (path.LastIndexOf('.') - pathName.Length));
    }
}
