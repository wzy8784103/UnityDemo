using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 用法：
/// 1.树状结构，设定好树之后，只需要维护叶子节点的红点数量即可，树会自动向上更新父节点
/// 2.每个系统需自行注册节点和更新节点，由于每个窗口的差异度很大，故系统只负责维护父节点刷新
/// 3.这里有几种特殊情况，需要每个窗口独自维护
///  (1)每个节点需要是设定RedPointNode.ERedTreeRefreshType，默认是窗口打开的时候会更新红点，但是有的窗口需要通信，有的窗口会在Init中更新，如果在打开窗口时
///       也更新就会有问题
///  (2)没有红点的，必须改变领取按钮状态的，红点路径写""
///  (3)类似七天任务的页面，右侧带红点的ui会随着点击天数切页刷新，所以需要自己单独维护一个字典，当点击天数的时候，手动刷新一下红点UI
/// </summary>
public class RedPointTree : SingletonBase<RedPointTree>
{
    public Dictionary<string, Dictionary<ERedTreeRefreshType, List<RedPointNode>>> windowName2NodeDic = new Dictionary<string, Dictionary<ERedTreeRefreshType, List<RedPointNode>>>();

    public void Refresh(string windowName, ERedTreeRefreshType refreshType)
    {
        if (windowName2NodeDic.ContainsKey(windowName) && windowName2NodeDic[windowName].ContainsKey(refreshType))
        {
            List<RedPointNode> list = windowName2NodeDic[windowName][refreshType];
            foreach(var item in list)
			{
                item.RefreshUI();
            } 
        }
    }

    public void Clear()
    {
        windowName2NodeDic.Clear();
    }
}

public class RedPointNode
{
    public static RedPointNode Create(RedPointNode parent, string redPointPath, ERedTreeRefreshType refreshType = ERedTreeRefreshType.OpenWindow, bool isShowCount = false)
    {
        RedPointNode node = new RedPointNode();
        node.Parent = parent;
        node.redPointPath = redPointPath;
        node.refreshType = refreshType;
        node.isShowCount = isShowCount;
        if (redPointPath != "")
        {
            node.windowName = redPointPath.Substring(0, redPointPath.IndexOf('/'));
            Dictionary<string, Dictionary<ERedTreeRefreshType, List<RedPointNode>>> dic = RedPointTree.Singleton.windowName2NodeDic;
            if (!dic.ContainsKey(node.windowName))
            {
                dic.Add(node.windowName, new Dictionary<ERedTreeRefreshType, List<RedPointNode>>());
            }
            if (!dic[node.windowName].ContainsKey(refreshType))
            {
                dic[node.windowName].Add(refreshType, new List<RedPointNode>());
            }
            dic[node.windowName][refreshType].Add(node);
        }
        return node;
    }

    //窗口名
    public string windowName;
    //红点路径，以窗口名开始
    public string redPointPath;
    //什么时候刷新
    public ERedTreeRefreshType refreshType;
    //是否在小红点内显示数量
    public bool isShowCount = false;

    private int redCount;
    public int RedCount
    {
        get
        {
            return redCount;
        }
        set
        {
            if (redCount != value)
            {
                int oldValue = redCount;
                redCount = value;
                //只有由0-1、1-0或显示数量时才刷新UI
                if (value == 0 || oldValue == 0 || isShowCount)
                {
                    RefreshUI();
                }
                //更新父节点红点数量
                if (Parent != null)
                {
                    int delta = value - oldValue;
                    Parent.RedCount += delta;
                }
            }
        }
    }

    private RedPointNode parent;
    public RedPointNode Parent
    {
        get
        {
            return parent;
        }
        set
        {
            parent = value;
        }
    }

    public void RefreshUI()
    {
        if (redPointPath == "")
        {
            return;
        }
        Transform redTf = GameNode.Singleton.UIParentTf.Find(redPointPath);
        if (redTf == null)
        {
            //Debug.LogError("红点系统路径填写错误:" + redPointPath);
            return;
        }
        //红点计数为0直接设置False就行了
        if (redCount == 0)
        {
            redTf.gameObject.SetActive(false);
            return;
        }
        redTf.gameObject.SetActive(true);
        if (isShowCount)
        {
            Text[] texts = redTf.GetComponentsInChildren<Text>();
            if (texts.Length == 0)
            {
                Debug.LogError("无显示红点数量的组件");
            }
            texts[0].text = RedCount.ToString();
        }
    }
}

public enum ERedTreeRefreshType
{
    None,
    OpenWindow, //打开窗口时刷新
    MessageOver //通信完刷新
}