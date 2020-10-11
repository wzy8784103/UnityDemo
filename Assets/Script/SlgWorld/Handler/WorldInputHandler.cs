using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInputHandler : WorldHandlerBase
{
	private Vector2 lastTouchPos;
	//最大偏移
	private const float maxDelta = 0.01f;

	private Camera camera;
	public override void OnInit()
	{
		camera = WorldCreator.Singleton.camera;
		ScriptBridge.Singleton.AddUpdate(OnUpdate, this);
	}
	private void OnUpdate()
	{
		if (UITools.IsRaycastUI()) return;

		if (InputManager.GetMouseButtonDown())
		{
			lastTouchPos = InputManager.GetMousePosition();
		}
		if(InputManager.GetMouseButtonUp())
		{
			if(Vector2.Distance(InputManager.GetMousePosition(), lastTouchPos) <= maxDelta)
			{
				Ray ray = camera.ScreenPointToRay(InputManager.GetMousePosition());
				//发个射线
				float enter = 100.0f;
				Plane plane = WorldCreator.Singleton.plane;
				if (plane.Raycast(ray, out enter))
				{
					DiamondVector2 dPos = DiamondCoordinates.WorldToDiamond(ray.GetPoint(enter));
					EventDispatcher.Singleton.NotifyListener(EventKey.WorldClickTile, dPos);
					//暂时没别的需求，直接打开窗口
					if (DiamondCoordinates.IsValid(dPos))
					{
						UIManager.Singleton.OpenWindow<UIWorldPopup>(dPos);
					}
				}
			}
		}
	}
}
