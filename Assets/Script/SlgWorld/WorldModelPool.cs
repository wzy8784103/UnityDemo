using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


/// <summary>
/// 地图所有不在视野中模型的缓存，结构大致如下：
/// cacheDic： key为模型路径，value为链表，因为同一个模型可能被加多次，所以用链表存储
/// head: 除上述每个模型的单独链表外，额外做一个全模型链表，用于超出上限时删除最先缓存的模型
/// 这样设计的话，插入 获取 删除 都是O(1)的时间复杂度
/// </summary>
public class WorldModelPool
{
    //缓存容器，key是路径，value是链表，里面连接的都是同一个路径的obj
    private Dictionary<string, CacheNode> cacheDic = new Dictionary<string, CacheNode>();
    //所有node的头结点，用于缓存超上限时移除，这个头结点一定是最先加入的
    private CacheNode head;
    private CacheNode tail;
    //缓存总上限
    private int capacity;
    //当前缓存数
    private int size;
    public WorldModelPool(int capacity)
    {
        this.capacity = capacity;
    }
    public void Put(string fullPath, GameObject obj)
    {
        //Debug.Log("Put==" + fullPath);
        //Debug.Log("obj==" + obj);
        //Debug.Log("size==" + size);
        //Debug.Log("isContain=" + AssetManager.LoadBase.AssetDic.ContainsKey(fullPath));
        //如果缓存最大容量为0，则直接卸载，不做任何处理
        if (capacity <= 0)
        {
            AssetManager.AssetLoad.UnLoadAsset(fullPath);
            GameObject.Destroy(obj);
            return;
        }
        
        //如果达到上限，则清理掉缓存的头结点
        if (size == capacity && head != null)
        {
			//Debug.Log("卸载==" + head.fullPath + ", name =" + head.value.name);
            GameObject.Destroy(head.value);
            string headPath = head.fullPath;
            DeleteNode(head);
            if (!cacheDic.ContainsKey(headPath) && fullPath != headPath)
			{
                AssetManager.AssetLoad.UnLoadAsset(headPath);
            }
        }

        CacheNode node = new CacheNode();
        node.fullPath = fullPath;
        node.value = obj;
        obj.transform.position = new Vector3(1000, 1000, 1000);
        //obj.SetActive(false);
        if (cacheDic.ContainsKey(fullPath))
        {
            //这里插入链表时，不用非插入到最后一个，直接插入到第一个
            cacheDic[fullPath].prevOfPath = node;
            node.nextOfPath = cacheDic[fullPath];
            cacheDic[fullPath] = node;
        }
        else
        {
            cacheDic.Add(fullPath, node);
        }
        if(size == 0)
        {
            head = node;
            tail = head;
        }
		else
		{
            node.prevOfAll = tail;
            tail.nextOfAll = node;
            tail = node;
        }
        size++;
  //      if(headPath == "")
		//{
  //          return;
		//}
  //      string str = "";
  //      CacheNode tmpNode = head;
  //      while (tmpNode != null)
  //      {
  //          int len = tmpNode.fullPath.Length;
  //          int last = tmpNode.fullPath.LastIndexOf('/');
  //          str += (tmpNode.fullPath.Substring(last, len - last) + ",");
  //          tmpNode = tmpNode.nextOfAll;
  //      }
  //      string str2 = "";
  //      CacheNode tmpNode2 = head;
  //      while (tmpNode2 != null)
  //      {
  //          int len = tmpNode2.fullPath.Length;
  //          int last = tmpNode2.fullPath.LastIndexOf('/');
  //          str2 += (tmpNode2.fullPath.Substring(last, len - last) + ",");
  //          tmpNode2 = tmpNode2.nextOfPath;
  //      }
  //      string str3 = "";
  //      CacheNode tmpNode3 = node;
  //      while (tmpNode3 != null)
  //      {
  //          int len = tmpNode3.fullPath.Length;
  //          int last = tmpNode3.fullPath.LastIndexOf('/');
  //          str3 += (tmpNode3.fullPath.Substring(last, len - last) + ",");
  //          tmpNode3 = tmpNode3.nextOfPath;
  //      }
  //      Debug.Log("UnLoadAsset==" + headPath);
  //      Debug.Log("str==" + str);
  //      Debug.Log("str2==" + str2);
  //      Debug.Log("str3==" + str3);
    }

    //删除节点比较复杂，需要考虑两个prev节点和两个next节点，单独写个方法
    public void DeleteNode(CacheNode node)
	{
        //Debug.Log("DeleteNode==" + node.fullPath);
        //注意！！！！！！
        //这里被我自己坑了，在插入cacheDic是倒着插入的
        //所以如果删除的是头结点的话，头结点指向的是链表的末尾，而不是开头，查了好久...
        if (node.prevOfPath == null && node.nextOfPath == null)
        {
            //如果移除的是cacheDic的最后一个，则直接从容器中移除
            cacheDic.Remove(node.fullPath);
        }
        else
        {
            if (node.nextOfPath == null)
            {
                
            }
            else
            {
                node.nextOfPath.prevOfPath = node.prevOfPath;
            }
            if (node.prevOfPath == null)
			{
                cacheDic[node.fullPath] = node.nextOfPath;
            }
			else
			{
                node.prevOfPath.nextOfPath = node.nextOfPath;
            }
        }

        bool isHead = false;
        bool isTail = false;
        if (node.prevOfAll == null)
        {
            //如果没有全局前置节点，则代表是全局头结点
            isHead = true;
        }
        else
        {
            node.prevOfAll.nextOfAll = node.nextOfAll;
        }
        if (node.nextOfAll == null)
        {
            isTail = true;
        }
        else
        {
            node.nextOfAll.prevOfAll = node.prevOfAll;
        }

        if(isHead && isTail)
		{
            head = null;
            tail = null;
        }
        else if(isHead)
		{
            head = head.nextOfAll;
        }
        else if(isTail)
        {
            tail = tail.prevOfAll;
        }
        size--;
    }

    public delegate void LoadCacheCallBack(GameObject obj, DiamondVector2 pos, string fullPath);
    int index = 1;
    public void Get(string fullPath, DiamondVector2 pos, LoadCacheCallBack action)
    {
        //如果缓存中有值则直接从缓存中拿
        if (cacheDic.ContainsKey(fullPath))
        {
            //直接拿第一个节点
            CacheNode node = cacheDic[fullPath];
            DeleteNode(node);
            action(node.value, pos, fullPath);
        }
        else
        {
            //Debug.Log("LoadAssetAsync==" + fullPath);
            //这里直接用lambda做，需要传的参数挺多的
            AssetManager.AssetLoad.LoadAssetAsync<GameObject>(fullPath, 
                (loadInfo) =>
                {
                    //可能还没加载完就被卸载了，所以这里做个处理
                    if (loadInfo.obj != null)
                    {
                        GameObject obj = GameObject.Instantiate(loadInfo.obj) as GameObject;
                        obj.name = index.ToString();
                        obj.transform.parent = WorldNode.ObjectParent;
                        index++;
                        action(obj, pos, loadInfo.path);
                    }
                }, 
                null);
        }
    }

    public void Release()
    {
        foreach (var kv in cacheDic)
        {
            CacheNode node = kv.Value;
            while (node != null)
            {
                AssetManager.AssetLoad.UnLoadAsset(node.fullPath);
                GameObject.Destroy(node.value);
                node = node.nextOfPath;
            }
        }
        cacheDic = null;
    }
}

public class CacheNode
{
    public string fullPath;
    public GameObject value;
    public CacheNode prevOfPath;
    public CacheNode nextOfPath;
    public CacheNode prevOfAll;
    public CacheNode nextOfAll;
}
