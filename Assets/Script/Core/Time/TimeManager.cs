using System;
using System.Collections.Generic;
public class TimeManager : SingletonBase<TimeManager>
{
    private long key = 0;
    private Dictionary<long, TimeInfo> timeDict = new Dictionary<long, TimeInfo>();

    //由于在遍历的时候有回调，但是无法保证不在回调里对原容器进行操作，所以这里添加两个临时容器，一个是添加的，一个是移除的
    private Dictionary<long, TimeInfo> tmpDict = new Dictionary<long, TimeInfo>();
    private HashSet<long> removeSet = new HashSet<long>();

    public override void InitSingleton()
    {
        ScriptBridge.Singleton.AddUpdate(Update, null);
    }

    public long AddTimer(TimeInfo info)
    {
        //key值永远都不重复
        key++;
        tmpDict.Add(key, info);
        return key;
    }
    public void RemoveTimer(ref int removeKey)
    {
        //如果临时容器中有就说明还没来得及添加，直接删除
        if (tmpDict.ContainsKey(removeKey))
        {
            tmpDict.Remove(removeKey);
            return;
        }
        if (timeDict.ContainsKey(removeKey))
        {
            removeSet.Add(removeKey);
        }
        removeKey = 0;
    }
    public TimeInfo GetTimeInfo(long key)
    {
        //先判断是否在移除列表中，因为可能移除列表和正常容器同时存在，即还没有删除的情况
        if (removeSet.Contains(key))
        {
            return null;
        }
        if (timeDict.ContainsKey(key))
        {
            return timeDict[key];
        }
        if (tmpDict.ContainsKey(key))
        {
            return tmpDict[key];
        }
        return null;
    }
    public long CreateTimerOfEnd(long endTime, Action<object> action, object arg = null)
    {
        TimeInfo info = new TimeInfo();
        info.endTime = endTime;
        info.action = action;
        info.arg = arg;
        return TimeManager.Singleton.AddTimer(info);
    }
    public long CreateTimerOfDuration(long duration, Action<object> action, object arg = null)
    {
        TimeInfo info = new TimeInfo();
        info.endTime = TimeTools.GetCurMilliseconds() + duration;
        info.action = action;
        info.arg = arg;
        return TimeManager.Singleton.AddTimer(info);
    }
    public void Clear()
    {
        tmpDict.Clear();
        timeDict.Clear();
    }

    private void RemoveTimeDict(long key)
    {
        if (timeDict.ContainsKey(key))
        {
            timeDict.Remove(key);
        }
    }
    private void Update()
    {
        if (tmpDict.Count > 0)
        {
            foreach (var kv in tmpDict)
            {
                timeDict.Add(kv.Key, kv.Value);
            }
            tmpDict.Clear();
        }
        if (removeSet.Count > 0)
        {
            foreach (var item in removeSet)
            {
                RemoveTimeDict(item);
            }
            removeSet.Clear();
        }
        if (timeDict.Count > 0)
        {
            foreach(var kv in timeDict)
            {
                TimeInfo info = kv.Value;
                if (info.endTime - TimeTools.GetCurMilliseconds() <= 0)
                {
                    removeSet.Add(kv.Key);
                    if (info.action != null)
                    {
                        info.action.Invoke(info.arg);
                    }
                }
            }
        }
    }
}
public class TimeInfo
{
    public long endTime;
    public Action<object> action;
    public object arg;
}