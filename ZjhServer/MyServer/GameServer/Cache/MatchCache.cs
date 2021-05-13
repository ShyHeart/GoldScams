using System.Collections.Generic;
using GameServer.DataBase;
using MyServer;

namespace GameServer.Cache
{
    /// <summary>
    /// 匹配缓存层
    /// </summary>
    public class MatchCache
    {
        /// <summary>
        /// 正在匹配的房间ID与用户ID的映射字典
        /// </summary>
        public  Dictionary<int,int>userIdRoomIdDic=new Dictionary<int, int>();

        /// <summary>
        /// 正在匹配的房间ID与之对应的房间数据模型之间的映射字典 
        /// </summary>
        public  Dictionary<int,MatchRoom>roomIdModelDic=new Dictionary<int, MatchRoom>();

        /// <summary>
        /// 重复使用的房间队列
        /// </summary>
        public Queue<MatchRoom>RoomQueue=new Queue<MatchRoom>();
        /// <summary>
        /// 线程安全的房间ID
        /// </summary>
        private  ThreadSafeInt roomId=new ThreadSafeInt(-1);

        /// <summary>
        /// 进入匹配房间
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public MatchRoom Enter(ClientPeer client)
        {
            //先遍历正在匹配的房间数据模型中有没有未满的房间。如果有，加进去
            foreach (var mr in roomIdModelDic.Values)
            {
                if (mr.IsFull())
                    continue;
                mr.Enter(client);
                userIdRoomIdDic.Add(client.Id,mr.roomId);
                return mr;
            }
            //如果执行到这里，代表正在匹配的房间数据模型字典中没有空位了，自己开一间房
            MatchRoom room = null;
            if (RoomQueue.Count > 0)
                room = RoomQueue.Dequeue();
            else
                room=new MatchRoom(roomId.Add_Get() );
            room.Enter(client);
            roomIdModelDic.Add(room.roomId,room);
            userIdRoomIdDic.Add(client.Id,room.roomId);
            return room;
        }

        /// <summary>
        /// 离开匹配房间
        /// </summary>
        /// <param name="userId"></param>
        public MatchRoom Leave(int userId)
        {
            int roomId = userIdRoomIdDic[userId];
            MatchRoom room = roomIdModelDic[roomId];
            room.Leave(DatabaseManager.GetClientPeerByUserId(userId));
            userIdRoomIdDic.Remove(userId);

            //如果房间为空，将房间加入到重用队列，从正在匹配的字典中移除掉
            if (room.IsEmpty())
            {
                roomIdModelDic.Remove(roomId);
                RoomQueue.Enqueue(room);
            }

            return room;
        }

        /// <summary>
        /// 是否在匹配房间里面
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsMatching(int userId)
        {
            return userIdRoomIdDic.ContainsKey(userId);

        }

        /// <summary>
        /// 获取房间所在的房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom GetRoom(int userId)
        {
            int roomId = userIdRoomIdDic[userId];
            return roomIdModelDic[roomId];

        }

        /// <summary>
        /// 销毁房间 游戏开始时调用
        /// </summary>
        /// <param name="room"></param>
        public void DestoryRoom(MatchRoom room)
        {
            roomIdModelDic.Remove(room.roomId);
            foreach (var client in room.ClientList)
            {
                userIdRoomIdDic.Remove(client.Id);

            }
            room.ClientList.Clear();
            room.ReadyUIdList.Clear();
            RoomQueue.Enqueue(room);
        }
    }
}
