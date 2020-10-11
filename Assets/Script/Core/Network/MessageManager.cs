using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 这个项目先只写的核心框架，超时重连什么的没往里加
/// </summary>
public class MessageManager : SingletonBase<MessageManager>
{
    private Socket socket;
    private MessageTransport transport;
    //回调容器
    private Dictionary<int, Action<MessageResponseBase>> actonDict = new Dictionary<int, Action<MessageResponseBase>>();
    private Dictionary<int, Action<MessageResponseBase>> broadCastActonDict = new Dictionary<int, Action<MessageResponseBase>>();
    //回调队列
    //private Queue<MessageResponseBase> resQueue = new Queue<MessageResponseBase>();
    private Queue<byte[]> resQueue = new Queue<byte[]>();

    public override void InitSingleton()
    {
        MessageRegister.Singleton.Register();
        ScriptBridge.Singleton.AddUpdate(OnUpdate, null);
    }

    private Action<int> connectAction;
    private int connectState;
    public void Connect(string host, int port, Action<int> action)
    {
        if (socket != null && socket.Connected)
        {
            DisConnect();
        }
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ie = new IPEndPoint(IPAddress.Parse(host), port);
        socket.BeginConnect(ie, (result) =>
        {
            try
            {
                //记得end
                socket.EndConnect(result);
                //实际发送和接受的处理类
                transport = new MessageTransport(socket);
                SetConnectAction(action, 0);
            }
            catch (SocketException ex)
            {
                Debug.LogError(ex.Message);
                SetConnectAction(action, 1);
            }
        }, socket);
    }
    //注意这里要扔到主线程去执行回调
    private void SetConnectAction(Action<int> action, int state)
    {
        connectAction = action;
        connectState = state;
    }

    public void Send(MessageRequestBase req, Action<MessageResponseBase> action)
    {
        AddResAction(req.id, action);
        //发送数据
        transport.Send(req);
    }

    public void AddResAction(int id, Action<MessageResponseBase> action)
    {
        if(action != null)
        {
            if (!actonDict.ContainsKey(id))
            {
                actonDict.Add(id, null);
            }
            actonDict[id] = action;
        }
    }

    public void AddBroadcastAction(int id, Action<MessageResponseBase> action)
    {
        if (action != null)
        {
            if (!broadCastActonDict.ContainsKey(id))
            {
                broadCastActonDict.Add(id, null);
            }
            broadCastActonDict[id] = action;
        }
    }
    public void AddCallBack(byte[] bytes)
    {
        resQueue.Enqueue(bytes);
    }

    private void OnUpdate()
    {
        if (resQueue.Count > 0)
        {
            while (resQueue.Count > 0)
            {
                InvokeCallBack(resQueue.Dequeue());
            }
            resQueue.Clear();
        }
        if(connectAction != null)
		{
            connectAction(connectState);
            connectAction = null;
        }
    }
    private void InvokeCallBack(byte[] bytes)
    {
        ByteBuffer buffer = new ByteBuffer(bytes.Length, true);
        buffer.WriteBytes(bytes);
        //接口id
        int id = buffer.ReadInt();
        MessageResponseBase res = MessageRegister.Singleton.GetMessageRes(id);
        if (res == null)
        {
            return;
        }
        try
        {
            res.Decode(buffer);
        }
        catch (Exception ex)
        {
            Debug.LogError("消息id:" + id + ",解析错误：" + ex.Message);
        }
        if (actonDict.ContainsKey(res.id))
        {
            //这里注意，需要先移除再回调，有的人通信写成了递归形式，如果不先移除，会导致他在回调里添加了回调，然后又被remove了
            Action<MessageResponseBase> action = actonDict[res.id];
            actonDict.Remove(res.id);
            action(res);
        }
        else if (broadCastActonDict.ContainsKey(res.id))
        {
            //这里注意，需要先移除再回调
            Action<MessageResponseBase> action = broadCastActonDict[res.id];
            broadCastActonDict.Remove(res.id);
            action(res);
        }
    }

    public void DisConnect()
    {
        //关闭Socket
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket = null;
        //所有没执行完的回调也不走了
        resQueue.Clear();
        actonDict.Clear();
        broadCastActonDict.Clear();
        //清理
        transport.Clear();
    }
}
