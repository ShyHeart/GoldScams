using MyServer;
using MySql.Data.MySqlClient;
using Protocol.Dto;
using System;
using System.Collections.Generic;

namespace GameServer.DataBase
{
    public class DatabaseManager
    {
        private static MySqlConnection sqlConnect;

        private static Dictionary<int, ClientPeer> idClientDic;

        private static RankListDto rankListDto;


        public static void StartConnect()
        {
            rankListDto = new RankListDto();
            idClientDic = new Dictionary<int, ClientPeer>();
            string conStr = "database=zjhgame;data source=127.0.0.1;port=3306;user=root;pwd=root";
            sqlConnect = new MySqlConnection(conStr);
            sqlConnect.Open();
        }
        /// <summary>
        /// 判断是否存在用户名
        /// </summary>
        public static bool IsExitUserName(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select UserName from userinfo where UserName=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;

        }
        /// <summary>
        /// 创建用户信息
        /// </summary>
        public static void CreatUser(string userName, string pwd)
        {
            //传入用户名 密码 在线 头像 金币有默认值不需要传入
            MySqlCommand cmd = new MySqlCommand("insert into userinfo set UserName=@name,PassWord=@pwd,Online=0,IconName=@iconName", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            cmd.Parameters.AddWithValue("pwd", pwd);
            //图片头像18张 random
            Random ran = new Random();
            int index = ran.Next(0, 19);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + index.ToString());
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 用户名和密码是否匹配
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public static bool IsMatch(string userName, string pwd)
        {
            MySqlCommand cmd = new MySqlCommand("select *from userinfo where UserName=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                //拿到当前的密码 是否等于传入的password
                bool result = reader.GetString("PassWord") == pwd;
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }
        /// <summary>
        /// 判断用户名是否在线
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsOnline(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select Online from userinfo where UserName=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                bool result = reader.GetBoolean("Online");
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }
        /// <summary>
        /// 登录上线
        /// </summary>
        /// <param name="userName"></param>
        public static void Login(string userName, ClientPeer client)
        {
            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=true where UserName=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            cmd.ExecuteNonQuery();


            //拿到id
            MySqlCommand cmd1 = new MySqlCommand("select * from userinfo where UserName=@name", sqlConnect);
            cmd1.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd1.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                int id = reader.GetInt32("Id");
                client.Id = id;
                client.UserName = userName;

                //判断id有没有进字典里面
                if (idClientDic.ContainsKey(id) == false)
                    idClientDic.Add(id, client);
                reader.Close();
            }
            reader.Close();
        }
        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="client"></param>
        public static void OffLine(ClientPeer client)
        {
            if (idClientDic.ContainsKey(client.Id))
                idClientDic.Remove(client.Id);

            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=false where Id=@id", sqlConnect);
            cmd.Parameters.AddWithValue("id", client.Id);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 使用用户id获取客户端连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClientPeer GetClientPeerByUserId(int id)
        {
            if (idClientDic.ContainsKey(id))
            {
                return idClientDic[id];
            }
            return null;
        }

        /// <summary>
        /// 构造用户信息传输模型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserDto CreatUserDto(int userId)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where Id=@id", sqlConnect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                //读取用户名 头像 金币 信息
                UserDto dto = new UserDto(userId, reader.GetString("UserName"), reader.GetString("IconName"), reader.GetInt32("Coin"));
                reader.Close();
                return dto;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// 获取排行榜信息
        /// </summary>
        /// <returns></returns>
        public static RankListDto GetRankListDto()
        {
            //从高到低进行排序  升序asc 降序desc
            MySqlCommand cmd = new MySqlCommand("select UserName,Coin from userinfo order by Coin desc", sqlConnect);
            MySqlDataReader reader = cmd.ExecuteReader();
            rankListDto.Clear();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    //读取一行就new一个dto
                    RankItemDto dto = new RankItemDto(reader.GetString("UserName"), reader.GetInt32("Coin"));
                    rankListDto.Add(dto);
                }
                reader.Close();
                return rankListDto;
            }
            reader.Close();
            return null;
        }
        /// <summary>
        /// 更新金币数量
        /// </summary>
        public static int UpdateCoinCount(int userId, int value)
        {
            MySqlCommand cmd = new MySqlCommand("select Coin from userinfo where Id=@id", sqlConnect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                int remainCoincount = reader.GetInt32("Coin");
                reader.Close();

                //更新后金币的数量
                //如果金币不够扣，直接为0
                int afterCoin = 0;
                if (value<0)
                {
                    if (remainCoincount<-value)
                    {
                        afterCoin = 0;
                    }
                    else
                    {
                        afterCoin = remainCoincount + value;
                    }
                }
                else
                {
                     afterCoin = remainCoincount + value;
                }
                MySqlCommand cmdUpdate = new MySqlCommand("update userinfo set Coin=@coin where Id=@id", sqlConnect);
                cmdUpdate.Parameters.AddWithValue("coin", afterCoin);
                cmdUpdate.Parameters.AddWithValue("id", userId);
                cmdUpdate.ExecuteNonQuery();

                return afterCoin;

            }
            reader.Close();
            return 0;
        }
    }
}
