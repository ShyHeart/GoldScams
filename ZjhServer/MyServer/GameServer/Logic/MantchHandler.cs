using MyServer;
using System;
using System.Collections.Generic;
using GameServer.Cache;
using GameServer.DataBase;
using Protocol.Code;
using Protocol.Dto;

namespace GameServer.Logic
{
    /// <summary>
    /// 开始游戏的委托
    /// </summary>
    /// <param name="clientList"></param>
    /// <param name="roomType"></param>
    public delegate void StartFight(List<ClientPeer> clientList, int roomType);

    public class MantchHandler : IHandler
    {
        /// <summary>
        /// 匹配房间缓存集合
        /// </summary>
        private List<MatchCache> matchCacheList = Caches.matchCacheList;

        public StartFight StartFight;

        public void Disconnect(ClientPeer client)
        {
            for (int i = 0; i < matchCacheList.Count; i++)
            {
                LeaveRoom(client,i);
            }
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case MatchCode.Enter_CREQ:
                    EnterRoom(client, (int) value);
                    break;
                case MatchCode.Leave_CREQ:
                    LeaveRoom(client, (int) value);
                    break;
                case MatchCode.Ready_CREQ:
                    Ready(client, (int) value);
                    break;
                case MatchCode.UnReady_CREQ:
                    UnReady(client, (int) value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void UnReady(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;


                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.UnReady(client.Id);
                room.Broadcast(OpCode.Match, MatchCode.UnReady_BRO, client.Id);

            });
        }
        /// <summary>
        /// 客户端发来的准备的请求
        /// </summary>
        /// <param name="client"></param>
        private void Ready(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;


                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.Ready(client.Id);
                room.Broadcast(OpCode.Match,MatchCode.Ready_BRO,client.Id);

                //全部都准备了，可以开始游戏了
                if (room.IsAllReady())
                {
                    StartFight(room.ClientList, roomType);
                    //通知房间内所有玩家，开始游戏
                    room.Broadcast(OpCode.Match,MatchCode.StartGame_BRO,null);
                    //销毁房间
                    matchCacheList[roomType].DestoryRoom(room);
                }
            });
        }
        /// <summary>
        /// 客户端进入房间的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void EnterRoom(ClientPeer client, int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //判断一下当前客户端连接对象是不是在匹配房间以内，如果在，则忽略
                if (matchCacheList[roomType].IsMatching(client.Id)) return;

                MatchRoom room = matchCacheList[roomType].Enter(client);
                //构造UserDto用户数据传输模型
                UserDto userDto = DatabaseManager.CreatUserDto(client.Id);
                //广播给房间内的所有玩家，有新的玩家进来了，参数：新进用户的UserDto
                room.Broadcast(OpCode.Match, MatchCode.Enter_BRO, userDto, client);

                //给客户端一个响应 参数：房间传输模型 包含房间内正在等待的玩家以及准备玩家的Id集合
                client.SendMsg(OpCode.Match, MatchCode.Enter_SRES, MakeMatchRoomDto(room));

                if (roomType == 0)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为10，顶注为100的房间");
                }
                if (roomType == 1)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为20，顶注为200的房间");
                }
                if (roomType == 2)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为50，顶注为500的房间");
                }

            });
        }
        private MatchRoomDto MakeMatchRoomDto(MatchRoom room)
        {
            MatchRoomDto dto = new MatchRoomDto();
            for (int i = 0; i < room.ClientList.Count; i++)
            {
                dto.Enter(DatabaseManager.CreatUserDto(room.ClientList[i].Id));
            }
            dto.readyUserIdList = room.ReadyUIdList;
            return dto;
        }
        /// <summary>
        /// 客户端离开的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void LeaveRoom(ClientPeer client, int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //不在匹配房间 忽略
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;

                MatchRoom room = matchCacheList[roomType].Leave(client.Id);
                room.Broadcast(OpCode.Match, MatchCode.Leave_BRO, client.Id);
            });
        }
    }
}
