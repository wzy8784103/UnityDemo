using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    private Dictionary<Type, UIBase> windowDic = new Dictionary<Type, UIBase>();
    private Dictionary<string, UIBase> name2WindowDic = new Dictionary<string, UIBase>();
    /// <summary>
    /// 一个窗口只允许打开一次
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public void OpenWindow<T>(object arg = null) where T : UIBase
    {
        if(IsOpen<T>())
		{
            Debug.Log("窗口已打开");
            return;
		}
        Type t = typeof(T);
        UIBase windowBase = Activator.CreateInstance(t) as UIBase;
        windowBase.Open(arg);
        AddDic(t, windowBase);
    }
    /// <summary>
    /// 获取window，未开启返回null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetWindow<T>() where T : UIBase
    {
        Type t = typeof(T);
        if (windowDic.ContainsKey(t))
        {
            return windowDic[t] as T;
        }
        return null;
    }
    /// <summary>
    /// 是否开启窗口，这里不管是否隐藏
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool IsOpen<T>() where T : UIBase
    {
        return windowDic.ContainsKey(typeof(T));
    }
    /// <summary>
    /// 清理所有窗口，用于重连等
    /// </summary>
    public void Clear()
	{
        List<UIBase> list = new List<UIBase>(windowDic.Values);
        foreach(var item in list)
		{
            item.Close();
        }
        windowDic.Clear();
        name2WindowDic.Clear();
    }

    public void RemoveDic(UIBase windowBase)
    {
        windowDic.Remove(windowBase.GetType());
        name2WindowDic.Remove(windowBase.GetWindowName());
    }
    private void AddDic(Type t, UIBase windowBase)
    {
        windowDic.Add(t, windowBase);
        name2WindowDic.Add(windowBase.GetWindowName(), windowBase);
    }
}
