using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldCreator : SingletonBase<WorldCreator>
{
	public Plane plane = new Plane(Vector3.up, Vector3.zero);
	public Camera camera = Camera.main;

	private List<WorldHandlerBase> handlerList;
	public void Init()
	{
		//数据先行
		WorldDataManager.Singleton.Init();
		//注册
		handlerList = new List<WorldHandlerBase>();
		handlerList.Add(new WorldCameraHandler());
		handlerList.Add(new WorldAOIChangeHandler());
		handlerList.Add(new WorldObjectHandler());
		handlerList.Add(new WorldServerSimulateHandler());
		handlerList.Add(new WorldTileHandler());
		handlerList.Add(new WorldInputHandler());
		foreach (var item in handlerList)
		{
			item.OnInit();
		}
		
	}
	public void ReleaseAll()
	{
		foreach (var item in handlerList)
		{
			item.Release();
		}
		WorldDataManager.Singleton.Release();
	}
}
