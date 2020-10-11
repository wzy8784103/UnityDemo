using System;
using System.Collections.Generic;

/// <summary>
/// 自动卸载，这里不考虑挂脚本的destory做，问题比较多
/// </summary>
public abstract class AutoReleaseBase
{
	#region 资源
	private Dictionary<string, int> assetCountDic = new Dictionary<string, int>();
	public void AssetRetain(string fullPath)
	{
		if (!assetCountDic.ContainsKey(fullPath))
		{
			assetCountDic.Add(fullPath, 1);
		}
		else 
		{
			assetCountDic[fullPath]++;
		}
	}
    private void AssetRelease()
    {
        if (assetCountDic.Count > 0)
		{
            foreach (var kv in assetCountDic)
            {
                for (int i = 0, count = kv.Value; i < count; i++)
                {
                    AssetManager.AssetLoad.UnLoadAsset(kv.Key);
                }
            }
            assetCountDic.Clear();
        }
	}
    #endregion

    #region 事件
    //事件记录
    private Dictionary<EventKey, Delegate> eventCountDict = new Dictionary<EventKey, Delegate>();
    public void EventRetain(EventKey key, Delegate d)
    {
        //这里不用考虑Key冲突的情况，不可能有出现监听两次的情况，直接崩给他看
        eventCountDict.Add(key, d);
    }
    private void EventRelease()
    {
        if(eventCountDict.Count > 0)
		{
            foreach (var kv in eventCountDict)
            {
                EventDispatcher.Singleton.RemoveDict(kv.Key, kv.Value);
            }
            eventCountDict.Clear();
        }
    }
    #endregion

    #region 脚本update
    private HashSet<Action> scriptUpdateSet = new HashSet<Action>();
    public void ScriptUpdateRetain(Action action)
    {
        //这里不用考虑Key冲突的情况，不可能有出现添加两次的情况，直接崩给他看
        scriptUpdateSet.Add(action);
    }
    private void ScriptUpdateRelease()
    {
        if(scriptUpdateSet.Count > 0)
		{
            foreach (var item in scriptUpdateSet)
            {
                ScriptBridge.Singleton.RemoveUpdate(item);
            }
            scriptUpdateSet.Clear();
        }
    }
    #endregion
    protected void ReleaseAll()
	{
        AssetRelease();
        EventRelease();
        ScriptUpdateRelease();
    }
}
