using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldAOIChangeHandler : WorldHandlerBase
{
	//全量list，存储所有aoi的状态
	private byte[] aoiStateBytes = new byte[AOICoordinates.aoiMaxCount * AOICoordinates.aoiMaxCount];

	private List<Vector2Int> oldAOIList = new List<Vector2Int>(10);
	private List<Vector2Int> newAOIList = new List<Vector2Int>(10);

	//发送的非活跃列表
	private List<Vector2Int> inactiveList = new List<Vector2Int>(10);
	//发送的活跃列表
	private List<Vector2Int> newactiveList = new List<Vector2Int>(10);

	public override void OnInit()
	{
		EventDispatcher.Singleton.AddListener<Vector3, Vector3, Vector3, Vector3>(EventKey.WorldCameraMove, OnCameraMove, this);
	}
	private void OnCameraMove(Vector3 posLB, Vector3 posLT, Vector3 posRT, Vector3 posRB)
	{
		//向外扩展的距离，这里和画tile一样，为了让实际范围比显示更大，这样拖动时不会露馅
		float extraDis = DiamondCoordinates.w * 2;

		//算出4个点对应的aoi，然后遍历所有aoi插入
		Vector2Int aoiLB = AOICoordinates.WorldToAOI(posLB + new Vector3(-extraDis, 0, -extraDis));
		Vector2Int aoiLT = AOICoordinates.WorldToAOI(posLT + new Vector3(-extraDis, 0, extraDis));
		Vector2Int aoiRT = AOICoordinates.WorldToAOI(posRT + new Vector3(extraDis, 0, extraDis));
		//Vector2Int aoiRB = DiamondCoordinates.WorldToAOI(posRB += new Vector3(extraDis, 0, -extraDis));

		newAOIList.Clear();
		inactiveList.Clear();
		newactiveList.Clear();
		for (int x = aoiLT.x; x <= aoiRT.x; x++)
		{
			for(int y = aoiLT.y; y <= aoiLB.y; y++)
			{
				Vector2Int aoiPos = new Vector2Int(x, y);
				if(!AOICoordinates.IsValid(aoiPos))
				{
					continue;
				}
				//Debug.Log("aoiPos==" + aoiPos);
				//如果从不活跃变为活跃，则加入新增列表
				if(aoiStateBytes[AOICoordinates.AOIToIndex(aoiPos)] == 0)
				{
					newactiveList.Add(aoiPos);
				}
				newAOIList.Add(aoiPos);
			}
		}
		//将旧的设置为非活跃
		foreach (var item in oldAOIList)
		{
			aoiStateBytes[AOICoordinates.AOIToIndex(item)] = 0;
		}
		//将新的设置为活跃
		foreach (var item in newAOIList)
		{
			aoiStateBytes[AOICoordinates.AOIToIndex(item)] = 1;
		}
		//重新遍历上次数据，查找所有非活跃的，加入非活跃发送列表
		foreach (var item in oldAOIList)
		{
			if(aoiStateBytes[AOICoordinates.AOIToIndex(item)] == 0)
			{
				inactiveList.Add(item);
			}
		}

		//LogHelper.Log(oldAOIList, "oldAOIList==");
		//LogHelper.Log(newAOIList, "newAOIList==");

		//新的变为旧的
		List<Vector2Int> tmp = oldAOIList;
		oldAOIList = newAOIList;
		newAOIList = tmp;
		//如果有新的aoi进入视野或者有旧的移除，就向后台发送通信
		if (newactiveList.Count > 0 || inactiveList.Count > 0)
		{
			//LogHelper.Log(newactiveList, "newactiveList:");
			//LogHelper.Log(inactiveList, "inactiveList:");
			EventDispatcher.Singleton.NotifyListener(EventKey.WorldAOIChange, newactiveList, inactiveList, UserModel.Singleton.userId);
		}
	}
}
