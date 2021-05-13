using GameServer.Logic;
using MyServer;
using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// 网络消息处理中心，分发消息到对应的模块
    /// </summary>
    public class NetMsgCenter : IApplication
    {
        private AccountHandler accountHandler = new AccountHandler();
        private MantchHandler mantchHandler = new MantchHandler();
        private ChatHandler chatHandler = new ChatHandler();
        private FightHandler fightHandler = new FightHandler();


        public NetMsgCenter ()
        {
            mantchHandler.StartFight += fightHandler.StatrFight;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        public void Disconnect(ClientPeer client)
        {
            fightHandler.Disconnect(client);
            chatHandler.Disconnect(client);
            mantchHandler.Disconnect(client);
            accountHandler.Disconnect(client);

        }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public void Receive(ClientPeer client, NetMsg msg)
        {
            //判断接收过来的Msg是那个类的
            //分发各种消息
            switch (msg.opCode)
            {

                case OpCode.Accout:
                    accountHandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Chat:
                    chatHandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Fight:
                    fightHandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Match:
                    mantchHandler.Receive(client, msg.subCode, msg.value);
                    break;
                default:
                    break;
            }
        }
    }
}
