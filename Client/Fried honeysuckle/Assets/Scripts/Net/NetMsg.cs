﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 网络消息类
/// 作用：每次发送消息都发送这个类，接收到消息后需要转换成这个类
/// </summary>
public class NetMsg
{
    /// <summary>
    /// 操作码
    /// </summary>
    public int OpCode { get; set; }
    /// <summary>
    /// 子操作码
    /// </summary>
    public int subCode { get; set; }
    /// <summary>
    /// 传递的参数
    /// </summary>
    public object value { get; set; }

    public NetMsg()
    {

    }
    public NetMsg(int opCode, int subCode, object value)
    {
        this.OpCode = opCode;
        this.subCode = subCode;
        this.value = value;

    }
    public void Change(int opCode, int subCode, object value)
    {
        //后续调用Change方法就好
        this.OpCode = opCode;
        this.subCode = subCode;
        this.value = value;
    }

}

