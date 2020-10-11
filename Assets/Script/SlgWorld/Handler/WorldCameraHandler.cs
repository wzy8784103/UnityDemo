using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldCameraHandler : WorldHandlerBase
{
	private const float speed = 1;
	//最小容忍距离，超过发送移动广播
	private float minMoveDis = 0.2f;
	private Vector2 lastTouchPos = Vector2.zero;

	private Camera camera;
	private Transform cameraTf;

	private Vector3 cameraPos = Vector3.zero;
	public override void OnInit()
	{
		camera = WorldCreator.Singleton.camera;
		cameraTf = camera.transform;
		ScriptBridge.Singleton.AddLateUpdate(OnUpdate, this);
		ScriptBridge.Singleton.AddLateUpdate(OnLateUpdate, this);
		LookAt(new Vector3(0, 0, -1400));
	}
	
	private void OnUpdate()
	{
		//TODO
		if (camera.transform.position != cameraPos && Vector3.Distance(camera.transform.position, cameraPos) >= minMoveDis)
		{
			//重新设置当前摄像机位置
			cameraPos = camera.transform.position;

			Vector3 posLB = Vector3.zero;
			Vector3 posLT = Vector3.zero;
			Vector3 posRT = Vector3.zero;
			Vector3 posRB = Vector3.zero;
			//从摄像机四个角发射线，求出4个角对应的世界坐标
			//这个没想到别的好办法
			float enter = 100.0f;
			Ray ray = camera.ViewportPointToRay(new Vector3(0, 0));
			Plane plane = WorldCreator.Singleton.plane;
			if (plane.Raycast(ray, out enter))
				posLB = ray.GetPoint(enter);
			ray = camera.ViewportPointToRay(new Vector3(0, 1));
			if (plane.Raycast(ray, out enter))
				posLT = ray.GetPoint(enter);
			ray = camera.ViewportPointToRay(new Vector3(1, 1));
			if (plane.Raycast(ray, out enter))
				posRT = ray.GetPoint(enter);
			ray = camera.ViewportPointToRay(new Vector3(1, 0));
			if (plane.Raycast(ray, out enter))
				posRB = ray.GetPoint(enter);
			//发送摄像机移动事件吗，参数为当前摄像机视野的射线 世界坐标
			EventDispatcher.Singleton.NotifyListener(EventKey.WorldCameraMove, posLB, posLT, posRT, posRB);
		}
	}

	private void OnLateUpdate()
	{
		if (UITools.IsRaycastUI()) return;
		//如果没按下就不走逻辑
		if (!InputManager.GetMouseButtonTouch()) return;

		if(InputManager.GetMouseButtonDown())
		{
			lastTouchPos = InputManager.GetMousePosition();
		}
		//移动距离要大于容忍距离才开始移动
		Vector3 delta = InputManager.GetMousePosition() - lastTouchPos;
		cameraTf.position -= new Vector3(delta.x * 0.01f, 0, delta.y * 0.01f);
		lastTouchPos = InputManager.GetMousePosition();
	}
	public void LookAt(Vector3 worldPos)
	{
		camera.transform.position = new Vector3(worldPos.x, camera.transform.position.y, worldPos.z - 4.56f);
	}
}
