using MyServer;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.DataBase;

namespace GameServer.Logic
{
    public class AccountHandler : IHandler
    {
        public void Disconnect(ClientPeer client)
        {
            DatabaseManager.OffLine(client);
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.Register_CREQ:  //注册
                    Register(client, value as AccountDto);
                    break;
                case AccountCode.Login_CREQ:  //登录
                    Login(client, value as AccountDto);
                    break;
                case AccountCode.GetUserInfo_CREQ: //用户信息
                    GetUserInfo(client);
                    break;
                case AccountCode.GetRankList_CREQ:  //排行榜
                    GetRankList(client);
                    break;
                case AccountCode.UpdateCoinCount_CREQ:   //更新金币
                    UpdateCoinCount(client, (int)value);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 客服端发来的更新金币数量的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="coinCount"></param>
        private void UpdateCoinCount(ClientPeer client, int coinCount)
        {
            //调用数据库更新金币的方法
            SingleExecute.Instance.Execute(() =>
            {
                int totaCoin = DatabaseManager.UpdateCoinCount(client.Id, coinCount);
                client.SendMsg(OpCode.Accout, AccountCode.UpdateCoinCount_SRES, totaCoin);
            });
        }
        /// <summary>
        /// 客户端获取排行榜的请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void GetRankList(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                RankListDto dto = DatabaseManager.GetRankListDto();
                client.SendMsg(OpCode.Accout, AccountCode.GetRankList_SRES, dto);
            });
        }
        /// <summary>
        /// 客户端获取用户信息的请求
        /// </summary>
        /// <param name="client"></param>
        private void GetUserInfo(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                UserDto dto = DatabaseManager.CreatUserDto(client.Id);
                client.SendMsg(OpCode.Accout, AccountCode.GetUserInfo_SRES, dto);
            });
        }
        /// <summary>
        /// 客户端登录的请求
        /// </summary>
        private void Login(ClientPeer client, AccountDto dto)
        {
            SingleExecute.Instance.Execute(() =>
            {

                if (DatabaseManager.IsExitUserName(dto.userName) == false)
                {
                    //用户名不存在
                    client.SendMsg(OpCode.Accout, AccountCode.Login_SRES, -1);
                    return;
                }
                if (DatabaseManager.IsMatch(dto.userName, dto.passWord) == false)
                {
                    //密码不正确
                    client.SendMsg(OpCode.Accout, AccountCode.Login_SRES, -2);
                    return;
                }
                if (DatabaseManager.IsOnline(dto.userName))
                {
                    //该账户已经在线
                    client.SendMsg(OpCode.Accout, AccountCode.Login_SRES, -3);
                    return;
                }
                DatabaseManager.Login(dto.userName, client);
                //登录成功
                client.SendMsg(OpCode.Accout, AccountCode.Login_SRES, 0);
            });
        }
        /// <summary>
        /// 客户端注册的处理
        /// </summary>
        /// <param name="dto"></param>
        public void Register(ClientPeer client, AccountDto dto)
        {
            //单线程执行
            //防止多个线程同时访问数据出错
            SingleExecute.Instance.Execute(() =>
            {
                //处理接收到的数据
                //用户名已被注册
                if (DatabaseManager.IsExitUserName(dto.userName))
                {
                    client.SendMsg(OpCode.Accout, AccountCode.Register_SRES, -1);//-1代表用户名已被注册
                    return;
                }
                //创建一条用户数据
                DatabaseManager.CreatUser(dto.userName, dto.passWord);
                client.SendMsg(OpCode.Accout, AccountCode.Register_SRES, 0);
            });

        }

    }
}
