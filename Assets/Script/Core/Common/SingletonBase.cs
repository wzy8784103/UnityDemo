using System;
using System.Collections.Generic;

public interface ISingletonRelease
{
    void Release();
}

public abstract class SingletonBase<T> : ISingletonRelease where T : SingletonBase<T>, new()
{
    private static T t;
    public static T Singleton
    {
        get
        {
            if(t == null)
            {
                t = new T();
                t.InitSingleton();
                SingletonRelease.releaseSet.Add(t);
            }
            return t;
        }
    }
    public virtual void InitSingleton()
    {

    }
    
    public void Release()
	{
        SingletonRelease.releaseSet.Remove(t);
        OnRelease();
        t = null;
	}
    public virtual void OnRelease()
	{

	}
}

//切换账号用的，可以清理所有单例
public static class SingletonRelease
{
    public static HashSet<ISingletonRelease> releaseSet = new HashSet<ISingletonRelease>();
    public static void ReleaseAll()
	{
        List<ISingletonRelease> list = new List<ISingletonRelease>(releaseSet);
        foreach(var item in list)
		{
            item.Release();
        }
	}
}

