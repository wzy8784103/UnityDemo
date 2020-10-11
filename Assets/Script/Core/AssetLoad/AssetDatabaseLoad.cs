using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

public class AssetDatabaseLoad : AssetLoadBase<AssetDatabaseLoad>
{
    #region 同步加载相关
    public override T LoadAsset<T>(string path)
    {
        if(IsLoadAsset(path))
        {
            AssetDic[path].Retain();
            return AssetInfo.GetRes<T>(path);
        }
        T t = AssetDatabase.LoadAssetAtPath<T>(path);
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
                list.Add(AssetLoadInfo.Create(item, arg));
            }
            action.Invoke(list);
        }
    }

    IEnumerator LoadAssetCoroutineHelper<T>(string path) where T : Object
    {
        if (IsLoadAsset(path))
        {
            AssetDic[path].Retain();
            yield break;
        }
        T t = AssetDatabase.LoadAssetAtPath<T>(path);
        AssetInfo.CreateInfo(path, t);
        yield return null;
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
        AssetDic[path].Release();
    }
    #endregion
}
#endif