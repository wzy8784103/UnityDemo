using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class UIBase : AutoReleaseBase
{
    public GameObject baseObject;
    public object arg;

    private string windowName;
    private static int curDepth = 100;
    private UIInfoScript infoScript;

    private static Stack<UIBase> openFullViewStack = new Stack<UIBase>();
    private static Dictionary<UIBase, Dictionary<UIBase, bool>> full2SubDic = new Dictionary<UIBase, Dictionary<UIBase, bool>>();

    public void Open(object arg = null)
    {
        this.arg = arg;
        windowName = this.ToString();
        baseObject = GameObject.Instantiate
			(AssetManager.AssetLoad.LoadAsset<GameObject>("Assets/Resources/prefab/" + windowName + ".prefab", this)
			, GameNode.Singleton.UIParentTf);
		baseObject.transform.localScale = Vector3.one;
		baseObject.transform.localPosition = Vector3.zero;
        baseObject.name = windowName;
        infoScript = baseObject.AddMissingComponent<UIInfoScript>();
        //多语言
        LanguageManager.SetPrefabLanguge(baseObject);
        //改depth
        DepthSet();
        //窗口层级管理
        LevelOpenCheck();
        //子类初始化
        OnInit();
        EventDispatcher.Singleton.NotifyListener(EventKey.WindowOpen, this);
        RedPointTree.Singleton.Refresh(windowName, ERedTreeRefreshType.OpenWindow);
    }

    public void Close()
    {
        EventDispatcher.Singleton.NotifyListener(EventKey.WindowClose, this);
        //清理
        ReleaseAll();
        //修改层级
        DepthMinus();
        //修改窗口层级
        LevelCloseCheck();
        //清理
        OnDestory();
        //清理容器
        UIManager.Singleton.RemoveDic(this);
        //删除
        GameObject.Destroy(baseObject);
    }

    public string GetWindowName()
	{
        return windowName;
    }

    private void DepthSet()
	{
        if(infoScript.showLevel == UIShowLevel.Nomatter)
		{
            return;
		}
        List<Canvas> canvaList = baseObject.GetComponentsInChildren<Canvas>().ToList();
        canvaList.Sort((x, y) => 
        {
            return x.sortingOrder.CompareTo(y.sortingOrder);
        });
        for(int i = 0; i < canvaList.Count; i++)
		{
            canvaList[i].sortingOrder = curDepth;
            curDepth += 1;
        }
	}
    private void DepthMinus()
	{
        if (infoScript.showLevel == UIShowLevel.Nomatter)
        {
            return;
        }
        Canvas[] canvas = baseObject.GetComponentsInChildren<Canvas>();
        curDepth -= canvas.Length;
    }

    public void Active()
	{
        baseObject.SetActive(true);
        OnActive();
    }
    public virtual void OnActive()
	{

	}
    public void Hide()
	{
        baseObject.SetActive(false);
        OnHide();
    }
    public virtual void OnHide()
    {

    }

    private void LevelOpenCheck()
	{
        if(infoScript.showLevel == UIShowLevel.Nomatter)
		{
            return;
		}
		switch (infoScript.showLevel)
		{
            case UIShowLevel.One:
				{
                    //如果打开的是全屏窗口，则把之前的隐藏掉
                    if (openFullViewStack.Count > 0)
					{
                        Dictionary<UIBase, bool> dic = full2SubDic[openFullViewStack.Peek()];
                        List<UIBase> list = new List<UIBase>(dic.Keys);
                        foreach (var view in list)
                        {
                            //记录之前的状态，用于显示的时候还原
                            dic[view] = view.baseObject.activeSelf;
                            view.Hide();
                        }
                    }
                    openFullViewStack.Push(this);
                    full2SubDic.Add(this, new Dictionary<UIBase, bool>()
                    {
                        { this, true }
                    });
                }
                break;
            case UIShowLevel.Two:
				{
                    if (openFullViewStack.Count > 0)
					{
                        Dictionary<UIBase, bool> dic = full2SubDic[openFullViewStack.Peek()];
                        dic.Add(this, true);
                    }
				}
                break;
        }
    }
    private void LevelCloseCheck()
    {
        if (infoScript.showLevel == UIShowLevel.Nomatter)
        {
            return;
        }
        switch (infoScript.showLevel)
        {
            case UIShowLevel.One:
                {
                    //从开启列表中移除
                    openFullViewStack.Pop();
                    //把之前的隐藏的打开
                    if (openFullViewStack.Count > 0)
					{
                        Dictionary<UIBase, bool> dic = full2SubDic[openFullViewStack.Peek()];
                        foreach (var kv in dic)
                        {
                            if (kv.Value)
                            {
                                kv.Key.Active();
                            }
                        }
                    }
                }
                break;
            case UIShowLevel.Two:
                {
                    int openFullViewCount = openFullViewStack.Count;
                    if (openFullViewCount > 0)
                    {
                        Dictionary<UIBase, bool> dic = full2SubDic[openFullViewStack.Peek()];
                        dic.Remove(this);
                    }
                }
                break;
        }
    }
    //子类通信完成调用，因为一些逻辑是通信完才会显示完全
    protected void MessageOver()
	{
        RedPointTree.Singleton.Refresh(windowName, ERedTreeRefreshType.MessageOver);
        EventDispatcher.Singleton.NotifyListener(EventKey.WindowMessageOver, this);
    }

    public abstract void OnInit();
    public abstract void OnDestory();
}
