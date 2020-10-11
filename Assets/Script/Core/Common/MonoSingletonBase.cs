using System;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingletonBase<T> : MonoBehaviour where T : MonoSingletonBase<T>, new()
{
    private static T t;
    public static T Singleton
    {
        get
        {
            if(t == null)
            {
                GameObject go = GameObject.Find(GameNode.gameStartName);
                if (go == null)
                {
                    go = new GameObject(GameNode.gameStartName);
                }
                GameObject.DontDestroyOnLoad(go);
				t = go.GetComponent<T>();
				if(t == null)
				{
					t = go.AddComponent<T>();
				}
                t.InitSingleton();
            }
            return t;
        }
    }
    public virtual void InitSingleton()
    {

    }
}
