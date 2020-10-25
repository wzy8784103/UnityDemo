using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldTileHandler : WorldHandlerBase
{
    //tile缓存列表
    private List<GameObject> cacheTileList = new List<GameObject>();
    //坐标对应的tile，存个容器好拿
    private Dictionary<DiamondVector2, GameObject> pos2TileObjDic = new Dictionary<DiamondVector2, GameObject>();

    //维护两个列表，初始分配内存，减少gc
    private List<DiamondVector2> oldTileList = new List<DiamondVector2>(150);
    private List<DiamondVector2> newTileList = new List<DiamondVector2>(150);

	public override void OnInit()
	{
		EventDispatcher.Singleton.AddListener<Vector3, Vector3, Vector3, Vector3> (EventKey.WorldCameraMove, OnCameraMove, this);
	}

    private void OnCameraMove(Vector3 posLB, Vector3 posLT, Vector3 posRT, Vector3 posRB)
    {
        //思路：将上一帧数据全部设置为卸载，再将本次设置为活跃，回头再遍历一次此列表，判断状态为卸载的即为差集
        foreach (var dPos in oldTileList)
        {
            WorldDataManager.Singleton.SetTileState(dPos.GetIndex(), WorldTileState.None);
        }
        //LogHelper.Output(newTileList, "newTileList1==");

        newTileList.Clear();
        DiamondCoordinates.ForeachByWorldPos(posLB, posLT, posRT, DiamondCoordinates.w * 2, 
            (dPos) =>
            {
                //将本次关注全部设置为活跃
                //Debug.Log(new Vector3(x + offset, 0, y).ToString() + "  " + dPos.ToString());
                int index = dPos.GetIndex();
                WorldDataManager.Singleton.SetTileState(index, WorldTileState.Active);
                //Debug.Log("dPos==" + dPos);
                newTileList.Add(dPos);
            });
        //重新遍历上次数据，查找所有状态为卸载的
        //LogHelper.Log(newTileList);
        foreach (var dPos in oldTileList)
        {
            int index = dPos.GetIndex();
            if(WorldDataManager.Singleton.GetTileState(index) == WorldTileState.None)
            {
                //Debug.LogError(item);
                GameObject cacheObj = pos2TileObjDic[dPos];
                cacheObj.SetActive(false);
                cacheTileList.Add(cacheObj);
                pos2TileObjDic.Remove(dPos);
                //将对应格子上的东西放到缓存列表里
                int objectId = WorldDataManager.Singleton.GetObjectId(index);
                if (objectId > 0)
                {
                    //Debug.Log("WorldObjectRemove==" + index + "=" + objectIdBytes[index]);
                    EventDispatcher.Singleton.NotifyListener(EventKey.WorldObjectRemove, dPos, WorldDataManager.Singleton.GetObjectResPath(index));
                    //这里不设置数据层，表现只管表现
                }
            }
        }
        //LogHelper.Log(newTileList, "newTileList==");
        //LogHelper.Output(loadList, "loadList==");
        foreach (var dPos in newTileList)
        {
            if (!pos2TileObjDic.ContainsKey(dPos))
            {
                GameObject obj = GetTile();
                obj.SetActive(true);
                obj.transform.localPosition = DiamondCoordinates.DiamondToWorld(dPos);
                pos2TileObjDic.Add(dPos, obj);
                int index = dPos.GetIndex();
                //如果数据中这个格子有物体，就刷出来
                int objectId = WorldDataManager.Singleton.GetObjectId(index);
                if (objectId > 0)
                {
                    //用当前最新的数据进行填充
                    EventDispatcher.Singleton.NotifyListener(EventKey.WorldObjectAdd, dPos, WorldDataManager.Singleton.GetObjectResPath(index));
                }
            }
        }
        //LogHelper.Output(newTileList, "newTileList3==");
        //新的变为旧的
        List<DiamondVector2> tmp = oldTileList;
        oldTileList = newTileList;
        newTileList = tmp;
    }
    private GameObject GetTile()
    {
        GameObject obj = null;
        if (cacheTileList.Count > 0)
        {
            obj = cacheTileList[0];
            cacheTileList.RemoveAt(0);
        }
        else
        {
            GameObject temp = AssetManager.AssetLoad.LoadAsset<GameObject>(ResPath.slgTilePath, this);
            obj = GameObject.Instantiate(temp);
            obj.transform.parent = WorldNode.TileParent;
            obj.transform.localScale = Vector3.one;
            obj.transform.localEulerAngles = new Vector3(-90, 180, 0);
            Renderer renderer = obj.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat("_Row", Random.Range(0, 8));
            block.SetFloat("_Col", Random.Range(0, 8));
            renderer.SetPropertyBlock(block);
        }
        return obj;
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }
}
