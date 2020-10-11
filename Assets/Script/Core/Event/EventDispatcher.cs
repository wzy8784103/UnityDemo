using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件分发类，这里因为字符串重构的复杂和易重复的原因,用枚举做key了，多人合作的时候需要划分一下
/// </summary>
public class EventDispatcher : SingletonBase<EventDispatcher>
{
	private Dictionary<EventKey, Delegate> eventDict = new Dictionary<EventKey, Delegate>();
	public void AddListener(EventKey eventKey, Action action, AutoReleaseBase autoRelease)
	{
		AddDict(eventKey, action, autoRelease);
	}
	public void AddListener<T0>(EventKey eventKey, Action<T0> action, AutoReleaseBase autoRelease)
	{
		AddDict(eventKey, action, autoRelease);
	}
	public void AddListener<T0, T1>(EventKey eventKey, Action<T0, T1> action, AutoReleaseBase autoRelease)
	{
		AddDict(eventKey, action, autoRelease);
	}
	public void AddListener<T0, T1, T2>(EventKey eventKey, Action<T0, T1, T2> action, AutoReleaseBase autoRelease)
	{
		AddDict(eventKey, action, autoRelease);
	}
	public void AddListener<T0, T1, T2, T3>(EventKey eventKey, Action<T0, T1, T2, T3> action, AutoReleaseBase autoRelease)
	{
		AddDict(eventKey, action, autoRelease);
	}

	public void RemoveListener(EventKey eventKey, Action action)
	{
		RemoveDict(eventKey, action);
	}
	public void RemoveListener<T0>(EventKey eventKey, Action<T0> action)
	{
		RemoveDict(eventKey, action);
	}
	public void RemoveListener<T0, T1>(EventKey eventKey, Action<T0, T1> action)
	{
		RemoveDict(eventKey, action);
	}
	public void RemoveListener<T0, T1, T2>(EventKey eventKey, Action<T0, T1, T2> action)
	{
		RemoveDict(eventKey, action);
	}
	public void RemoveListener<T0, T1, T2, T3>(EventKey eventKey, Action<T0, T1, T2, T3> action)
	{
		RemoveDict(eventKey, action);
	}

	public void NotifyListener(EventKey eventKey)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			(eventDict[eventKey] as Action)();
		}
		else
		{
			LogError(eventKey);
		}
	}
	public void NotifyListener<T0>(EventKey eventKey, T0 arg0)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			(eventDict[eventKey] as Action<T0>)(arg0);
		}
		else
		{
			LogError(eventKey);
		}
	}
	public void NotifyListener<T0, T1>(EventKey eventKey, T0 arg0, T1 arg1)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			//Debug.Log(eventKey);
			//LogHelper.Output(eventDict);
			(eventDict[eventKey] as Action<T0, T1>)(arg0, arg1);
		}
		else
		{
			LogError(eventKey);
		}
	}
	public void NotifyListener<T0, T1, T2>(EventKey eventKey, T0 arg0, T1 arg1, T2 arg2)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			(eventDict[eventKey] as Action<T0, T1, T2>)(arg0, arg1, arg2);
		}
		else
		{
			LogError(eventKey);
		}
	}
	public void NotifyListener<T0, T1, T2, T3>(EventKey eventKey, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			(eventDict[eventKey] as Action<T0, T1, T2, T3>)(arg0, arg1, arg2, arg3);
		}
		else
		{
			LogError(eventKey);
		}
	}

	private void LogError(EventKey eventKey)
	{
		//UnityEngine.Debug.LogError("事件==" + eventKey + "不存在");
	}

	private void AddDict(EventKey eventKey, Delegate d, AutoReleaseBase autoRelease)
	{
		if (!eventDict.ContainsKey(eventKey))
		{
			eventDict.Add(eventKey, null);
		}
		else
		{
			//排查一下重复的，防止重复加
			Delegate[] delegates = eventDict[eventKey].GetInvocationList();
			foreach (var item in delegates)
			{
				if (item == d)
				{
					UnityEngine.Debug.LogWarning("重复==" + d);
					return;
				}
			}
		}
		eventDict[eventKey] = Delegate.Combine(eventDict[eventKey], d);
		Retain(eventKey, d, autoRelease);
	}
	public void RemoveDict(EventKey eventKey, Delegate d)
	{
		if (eventDict.ContainsKey(eventKey))
		{
			eventDict[eventKey] = Delegate.RemoveAll(eventDict[eventKey], d);
			if (eventDict[eventKey] == null)
			{
				eventDict.Remove(eventKey);
			}
		}
	}

	private void Retain(EventKey eventKey, Delegate d, AutoReleaseBase autoRelease)
	{
		if (autoRelease != null)
		{
			autoRelease.EventRetain(eventKey, d);
		}
	}
}

//做一个统一枚举，所有key都写到这里，防止乱写string导致重复和不好查
public enum EventKey
{
    //0-1000 UI
	WindowOpen,			//打开窗口
	WindowClose,		//关闭窗口
	WindowMessageOver,	//窗口通信完成

    //1001-2000 world
    WorldObjectAdd = 1001,	//大地图物体加载
    WorldObjectRemove,		//大地图物体移除
	WorldServerDataGet,		//从服务器获取数据
	WorldServerDataSend,	//服务器返回数据
	WorldClickTile,         //点到一个地图块

	WorldCameraMove,		//大地图摄像机移动
	WorldAOIChange,		//客户端通知服务端修改aoi中自己的值
}

