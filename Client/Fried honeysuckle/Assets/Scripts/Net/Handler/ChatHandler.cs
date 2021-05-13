using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using UnityEngine;

public class ChatHandler : BaseHandler
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case ChatCode.BRO:
                EventCenter.Broadcast(EventDefine.ChatBRO,(ChatDto)value);
                break;
            default:
                break;
        }
    }


}
