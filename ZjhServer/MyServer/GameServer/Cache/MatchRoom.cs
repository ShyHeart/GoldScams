using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServer;

namespace GameServer.Cache
{
    /// <summary>
    /// 匹配房间
    /// </summary>
    public class MatchRoom
    {
        /// <summary>
        /// 房间ID，唯一标识符
        /// </summary>
        public  int roomId { get; private set; }

        /// <summary>
        /// 房间内的玩家
        /// </summary>
        public List<ClientPeer> ClientList { get; private set; }

        /// <summary>
        /// 房间内准备的玩家ID列表
        /// </summary>
        public List<int> ReadyUIdList { get; set; }

        public MatchRoom(int Id)
        {
            roomId = Id;
            ClientList=new List<ClientPeer>();
            ReadyUIdList=new List<int>();
        }

        /// <summary>
        /// 获取房间是否满了
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return ClientList.Count == 3;
        }

        /// <summary>
        /// 获取房间是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return ClientList.Count == 0;
        }

        /// <summary>
        /// 获取玩家是否都准备，如果返回值为true 就可以开始游戏
        /// </summary>
        /// <returns></returns>
        public bool IsAllReady()
        {
            return ReadyUIdList.Count == 3;
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        public void Enter(ClientPeer client)
        {
            ClientList.Add(client);
        }
        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="client"></param>
        public void Leave(ClientPeer client)
        {
            ClientList.Remove(client);
            //判断准备中的玩家有没有离开房间的
            if (ReadyUIdList.Contains(client.Id))
            {
                ReadyUIdList.Remove(client.Id);
            }
        }
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId)
        {
            ReadyUIdList.Add(userId);
        }
        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId)
        {
            ReadyUIdList.Remove(userId);
        }
        /// <summary>
        /// 广播发消息
        /// </summary>
        /// <param name="exceptcClient">剔除的玩家</param>
        public void Broadcast(int opcode,int subcode,object value,ClientPeer exceptcClient=null)
        {
            NetMsg msg=new NetMsg(opcode,subcode,value);
            //转换字节数组
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach (var client in ClientList)
            {
                if (client==exceptcClient)
                    continue;
                client.SendMsg(packet);
            }

        }
    }
}
