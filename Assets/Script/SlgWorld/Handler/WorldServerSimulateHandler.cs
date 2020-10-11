using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// 地图基本概述：
/// 1. 地图块总共1000 X 1000，即一百万块
/// 2. 本次demo的地图块、怪的位置都采用初始化时做随机来实现，如果做配置的话工作量比较大，毕竟demo只是方便看一下代码
/// 3. 
/// </summary>
public class WorldServerSimulateHandler : WorldHandlerBase
{
	//全地图块类型 0空地 1有怪
	private byte[] tileTypeBytes = new byte[DiamondCoordinates.maxX * DiamondCoordinates.maxY];
	//地图块物体对应的id
	private byte[] objectIdBytes = new byte[DiamondCoordinates.maxX * DiamondCoordinates.maxY];

	//aoi index对应的用户id集合，因为要频繁的增删，所以先用set了
	//每个用户会存当前所在的aoi,当用户发生数据变化时，反查到对应aoi然后发给所有对应玩家
	private Dictionary<Vector2Int, HashSet<long>> aoi2UserDic = new Dictionary<Vector2Int, HashSet<long>>();
	public override void OnInit()
	{
		//模拟服务器做一些假数据
		//每隔一位放一个怪
		byte origin = 1;
		byte id = origin;
		byte max = 20;
		for(int i = 0, count = tileTypeBytes.Length; i < count; i++)
		{
			//if(i % 2 == 0)
			//{
			//	continue;
			//}
			//1为有怪
			byte tag = (byte)UnityEngine.Random.Range(0, 2);
			tileTypeBytes[i] = tag;
			if(tag == 1)
			{
				objectIdBytes[i] = (byte)UnityEngine.Random.Range(1, 23);
			}
			id++;
			if(id > max)
			{
				id = origin;
			}
		}
		//监听客户端aoi发生改变
		EventDispatcher.Singleton.AddListener<List<Vector2Int>, List<Vector2Int>, long>(EventKey.WorldAOIChange, OnAOIChange, this);
	}

	//正常userid服务端通过session拿，这里因为是模拟所以就客户端发了
	private void OnAOIChange(List<Vector2Int> newactiveList, List<Vector2Int> inactiveList, long userId)
	{
		//旧的移除
		foreach(var item in inactiveList)
		{
			if (aoi2UserDic.ContainsKey(item))
			{
				aoi2UserDic[item].Remove(userId);
			}
		}

		//当客户端第一次注册新的aoi时，将新的所有信息返还给客户端
		if (newactiveList.Count > 0)
		{
			List<WorldDataDto> result = new List<WorldDataDto>();
			float halfAOICellWidth = AOICoordinates.aoiCellWidth / 2;
			//新的添加
			foreach (var item in newactiveList)
			{
				if (!aoi2UserDic.ContainsKey(item))
				{
					aoi2UserDic.Add(item, new HashSet<long>());
				}
				aoi2UserDic[item].Add(userId);
				//根据块中心找到4个角
				Vector3 worldPos = AOICoordinates.AOIToWorldPos(item);
				Vector3 posLB = worldPos + new Vector3(-halfAOICellWidth, 0, -halfAOICellWidth);
				Vector3 posLT = worldPos + new Vector3(-halfAOICellWidth, 0, halfAOICellWidth);
				Vector3 posRT = worldPos + new Vector3(halfAOICellWidth, 0, halfAOICellWidth);
				Vector3 posRB = worldPos + new Vector3(halfAOICellWidth, 0, -halfAOICellWidth);
				//CreateGameObject2(worldPos);
				//CreateGameObject(posLB);
				//CreateGameObject(posLT);
				//CreateGameObject(posRT);
				//CreateGameObject(posRB);
				//遍历当前aoi的所有点，发给客户端，实际遍历时比正常aoi大一点，防止边界问题
				DiamondCoordinates.ForeachByWorldPos(posLB, posLT, posRT, DiamondCoordinates.w,
				//DiamondCoordinates.ForeachByWorldPos(posLB, posLT, posRT, 0,
						(dPos) =>
						{
							//Debug.Log("ForeachByWorldPos dPos==" + dPos);
							int index = dPos.GetIndex();
							WorldDataDto dto = new WorldDataDto();
							dto.index = index;
							//Demo只做怪物
							dto.type = WorldObjectType.Monster;
							dto.id = objectIdBytes[index];
							//50%概率随便随机一个怪，模拟大地图物体会刷新
							//dto.id = UnityEngine.Random.Range(0, 2) == 0 ? (byte)0 : (byte)UnityEngine.Random.Range(1, 23);
							//dto.id = UnityEngine.Random.Range(0, 2) == 0 ? (byte)0 : (byte)UnityEngine.Random.Range(1, 5);
							//dto.id = dPos.x % 2 == 0 ? (byte)0 : (byte)2;
							//dto.id = 2;
							result.Add(dto);
						});
			}
			TimeManager.Singleton.CreateTimerOfDuration((long)(Random.Range(0.2f, 0.5f) * 1000), (arg) =>
			{
				EventDispatcher.Singleton.NotifyListener(EventKey.WorldServerDataSend, result);
			});
		}
	}

	private void CreateGameObject(Vector3 pos)
	{
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = pos;
	}
	private void CreateGameObject2(Vector3 pos)
	{
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		cube.transform.position = pos;
	}
}

public class WorldDataDto
{
    public int index;
	public WorldObjectType type;
    public byte id;
	public override string ToString()
	{
		return "(" + index + "= type:" + type + ",id:" + id + ")";
	}
}

public enum TileType
{
	None,
	Monster,
}
