using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
public class MessageTransport
{
    private Socket socket;
    private StateObject state;
    private ByteBuffer buffer;
    //上一条通信回来之后，才可以发下一条
    private bool IsSending = false;
    private Queue<MessageRequestBase> sendQueue = new Queue<MessageRequestBase>();
    public MessageTransport(Socket socket)
    {
        this.socket = socket;
        state = new StateObject();
        buffer = new ByteBuffer(true);
        BeginRecive();
    }

    public void Send(MessageRequestBase req)
    {
        //这里需要等待上一次发完，一般不会出现，因为发送时会屏蔽触摸，但是防止一些update每隔一段时间和后台请求
        if(IsSending)
        {
            sendQueue.Enqueue(req);
            return;
        }
        IsSending = true;
        ByteBuffer buffer = new ByteBuffer(true);
        //预先插入一个长度预留，数值随便写的
        buffer.WriteInt(0);
        //接口id
        buffer.WriteInt(req.id);
        req.Encode(buffer);
        byte[] bytes = buffer.ToArray();
        //把开头的预插入的长度修改为正确值,4为预先插入的长度
        byte[] lenBytes = BitConverter.GetBytes(bytes.Length - 4);
        //记得大小端翻转
        buffer.Reverse(lenBytes);
        //把开头的预插入的长度设为正确值
        for (int i = 0; i < lenBytes.Length; i++)
        {
            bytes[i] = lenBytes[i];
        }
        //发送
        if (socket.Connected)
        {
            socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, OnSend, null);
        }
        else
        {
            //TODO
            Debug.LogError("socket链接中断");
        }
    }
    private void OnSend(IAsyncResult asyncResult)
    {
        socket.EndSend(asyncResult);
    }

    public void BeginRecive()
    {
        socket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnReceive, state);
    }
    private void OnReceive(IAsyncResult asyncResult)
    {
        StateObject state = (StateObject)asyncResult.AsyncState;
        try
        {
            int totalLen = socket.EndReceive(asyncResult);
            //这里得处理流传输
            if (totalLen > 0)
            {
                //数据放入缓存
                buffer.WriteBytes(state.buffer, totalLen);
                while(buffer.GetCanRead() > 0)
                {
                    //发通信时加了个长度头，用于判断是否能拼完
                    int len = buffer.PeekInt();
                    //如果完整的长度比buffer中可读块大，则代表没接完呢，继续接
                    if (len > buffer.GetCanRead())
                    {
                        break;
                    }
                    //长度真正的读出来
                    buffer.ReadInt();
                    byte[] bytes = buffer.ReadBytes(len);
                    buffer.Reset();
                    AddMainThread(bytes);
                }
                BeginRecive();
            }
            else
            {
                //TODO
                Debug.LogError("无数据");
            }
        }
        //socket close时会回调begin，所以这里需要单catch一个，并且不需要做处理，参考
        //https://stackoverflow.com/questions/37135733/after-disposing-async-socket-net-callbacks-still-get-called
        catch (ObjectDisposedException ex)
        {
            Debug.Log(ex);
            //Debug.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            //TODO
            Debug.LogError(ex.Message);
        }
    }
    //处理数据
    private void AddMainThread(byte[] bytes)
    {
        //加入列表中等待主线程刷新
        MessageManager.Singleton.AddCallBack(bytes);
        IsSending = false;
        //消息回来之后继续处理未发送完的数据
        if (sendQueue.Count > 0)
        {
            Send(sendQueue.Dequeue());
        }
    }

    public void Clear()
    {
        IsSending = false;
        sendQueue.Clear();
    }
}

public class StateObject
{
    public const int BUFFER_SIZE = 1024;
    public byte[] buffer = new byte[BUFFER_SIZE];
}