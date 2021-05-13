using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientPeer 
{
    private Socket clientSocket;
    public NetMsg msg;

    public ClientPeer()
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            msg = new NetMsg();

        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

    }
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip,int port)
    {
        try
        {
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Debug.Log("连接服务器成功");
            StartReceive();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    #region 接收数据

    /// <summary>
    /// 数据暂存区
    /// </summary>
    private byte[] receiveBuffer = new byte[1024];
    /// <summary>
    /// 数据缓存
    /// </summary>
    private List<byte> receiveCache = new List<byte>();
    /// <summary>
    /// 是否正在处理接收到的数据
    /// </summary>
    private bool isProcessingReceive = false;
    /// <summary>
    /// 存放消息的队列
    /// </summary>
    public Queue<NetMsg> netMsgQueue = new Queue<NetMsg>();
    /// <summary>
    /// 开始接收数据
    /// </summary>
    private void StartReceive()
    {
        if (clientSocket == null && clientSocket.Connected == false)
        {
            Debug.LogError("没有连接成功，无法接收消息");
            return;
        }
        clientSocket.BeginReceive(receiveBuffer, 0, 1024, SocketFlags.None, ReceiveCallback, clientSocket);
    }
    /// <summary>
    /// 开始接收完成后的回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallback(IAsyncResult ar)
    {
        int length = clientSocket.EndReceive(ar);
        byte[] data = new byte[length];
        Buffer.BlockCopy(receiveBuffer, 0, data, 0, length);
        receiveCache.AddRange(data);

        //判断有没有在处理数据，没有就处理 一边接收一边处理
        if (isProcessingReceive==false)
        ProcessReceive();
        StartReceive();
    }
    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    private void ProcessReceive()
    {
        isProcessingReceive = true;
        //从缓存区里取出完整的包 转换成Msg
        byte[] packet = EncodeTool.DecodePacket(ref receiveCache);
        //判断包是不是空的
        if (packet == null)
        {
            isProcessingReceive = false;
            return;
        }
        NetMsg msg = EncodeTool.DecodeMsg(packet);
        //把转换到的消息存放到消息队列里
        netMsgQueue.Enqueue(msg);
        ProcessReceive();  //递归调用
    }
    #endregion

    #region 发送消息
    /// <summary>
    /// 发送数据
    /// </summary>
    public void SendMsg(int opCode,int subCode,object value)
    {
        msg.Change(opCode, subCode, value);
        Debug.Log("发送数据");
        SendMsg(msg);
    }
    public void SendMsg(NetMsg msg)
    {
        try
        {
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            clientSocket.Send(packet);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    #endregion

}
