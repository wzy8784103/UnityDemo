using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 地图怪物资源点资源等管理，添加，刷新，移除什么的
/// </summary>
public class WorldObjectHandler : WorldHandlerBase
{
    private class CacheObjectInfo
	{
        public GameObject obj;
        public string fullPath;
	}

    //缓存数量(不在视野内的缓存的数量)
    private int capacity = 20;
    private WorldModelPool cacheManager;

    private Dictionary<DiamondVector2, string> preLoadDic = new Dictionary<DiamondVector2, string>();
    //存一个地图块和object对应关系，只存缓存中的
    private Dictionary<DiamondVector2, CacheObjectInfo> tile2CacheDic = new Dictionary<DiamondVector2, CacheObjectInfo>();
    //由于是异步加载，有可能导致没加载完的时候就被移除了，这时逻辑就乱了，value是计数，因为可能一个资源多次
    private Dictionary<DiamondVector2, int> earlyRemoveDic = new Dictionary<DiamondVector2, int>();
    public override void OnInit()
    {
        cacheManager = new WorldModelPool(capacity);
        EventDispatcher.Singleton.AddListener<DiamondVector2, string>(EventKey.WorldObjectAdd, OnObjectAdd, this);
        EventDispatcher.Singleton.AddListener<DiamondVector2, string>(EventKey.WorldObjectRemove, OnObjectRemove, this);
    }
    private void OnObjectAdd(DiamondVector2 pos, string fullPath)
    {
		cacheManager.Get(fullPath, pos, OnLoadOver);
    }
    private void OnLoadOver(GameObject obj, DiamondVector2 pos, string fullPath)
    {
        //防止拖动过快导致还没加载完就被移除了,所以如果包含，证明已经被卸载了，则直接归入缓存中
        if (earlyRemoveDic.ContainsKey(pos))
		{
            //Debug.Log("earlyRemoveSet==" + fullPath);
            cacheManager.Put(fullPath, obj);
            tile2CacheDic.Remove(pos);
            earlyRemoveDic[pos]--;
            if(earlyRemoveDic[pos] == 0)
			{
                earlyRemoveDic.Remove(pos);
            }
            return;
		}
        //如果这个位置加载过，并且不是同一个物体，则把之前的删除
        if (tile2CacheDic.ContainsKey(pos))
		{
			OnObjectRemove(pos, tile2CacheDic[pos].fullPath);
        }

        //Debug.Log("OnLoadOver==" + fullPath);
        obj.transform.localEulerAngles = new Vector3(0, 180, 0);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = DiamondCoordinates.DiamondToWorld(pos);
        obj.SetActive(true);
        CacheObjectInfo info = new CacheObjectInfo();
        info.obj = obj;
        info.fullPath = fullPath;
        tile2CacheDic.Add(pos, info);
        //Debug.Log("OnLoadOver==" + obj.transform.localPosition);
    }
    private void OnObjectRemove(DiamondVector2 pos, string fullPath)
    {
        //Debug.Log("OnObjectRemove pos==" + pos);
        //如果移除的物体还没被加载完，则加入移除容器中，待加载完后做处理
        if (!tile2CacheDic.ContainsKey(pos))
		{
            //Debug.Log("earlyRemoveSet==" + pos + ", path==" + fullPath);
            if(!earlyRemoveDic.ContainsKey(pos))
			{
                earlyRemoveDic.Add(pos, 0);
            }
            earlyRemoveDic[pos]++;
            return;
        }
        //Debug.Log("OnObjectRemove==" + pos + ", path==" + fullPath);
        //这里要加个判断，因为这里存的东西可能之前就被移除了
        if (tile2CacheDic[pos].obj != null)
		{
            cacheManager.Put(fullPath, tile2CacheDic[pos].obj);
        }
        tile2CacheDic.Remove(pos);
    }
    public override void OnRelease()
    {
        cacheManager.Release();
    }
}
