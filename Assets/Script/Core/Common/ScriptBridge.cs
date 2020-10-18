using System;
using System.Collections.Generic;
/// <summary>
/// 主要用于不是脚本的类想用update,协程等
/// 容器全用list做，保证有序性
/// </summary>
public class ScriptBridge : MonoSingletonBase<ScriptBridge>
{
    private List<Action> updateActionList = new List<Action>();
    private List<Action> updateRemoveList = new List<Action>();
    private List<Action> updatePreList = new List<Action>();

    private List<Action> lateUpdateActionList = new List<Action>();
    private List<Action> lateUpdateRemoveList = new List<Action>();
    private List<Action> lateUpdatePreList = new List<Action>();

    public void AddUpdate(Action action, AutoReleaseBase autoRelease)
    {
        if(autoRelease != null)
		{
            autoRelease.ScriptUpdateRetain(action);
        }
        updatePreList.Add(action);
    }
    public void RemoveUpdate(Action action)
    {
        updateRemoveList.Add(action);
    }
    public void AddLateUpdate(Action action, AutoReleaseBase autoRelease)
    {
        if (autoRelease != null)
        {
            autoRelease.ScriptUpdateRetain(action);
        }
        lateUpdatePreList.Add(action);
    }
    public void RemoveLateUpdate(Action action)
    {
        lateUpdateRemoveList.Add(action);
    }
    private void Update()
    {
        if (updatePreList.Count > 0)
        {
            foreach (var item in updatePreList)
            {
                updateActionList.Add(item);
            }
            updatePreList.Clear();
        }
        foreach (var item in updateActionList)
        {
            item();
        }
        if(updateRemoveList.Count > 0)
        {
            foreach (var item in updateRemoveList)
            {
                updateActionList.Remove(item);
            }
            updateRemoveList.Clear();
        }
    }

	private void LateUpdate()
	{
        if (lateUpdatePreList.Count > 0)
        {
            foreach (var item in lateUpdatePreList)
            {
                lateUpdateActionList.Add(item);
            }
            lateUpdatePreList.Clear();
        }
        foreach (var item in lateUpdateActionList)
        {
            item();
        }
        if (lateUpdateRemoveList.Count > 0)
        {
            foreach (var item in lateUpdateRemoveList)
            {
                lateUpdateActionList.Remove(item);
            }
            lateUpdateRemoveList.Clear();
        }
    }
}
