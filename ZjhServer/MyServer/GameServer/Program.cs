using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServer;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerPree server = new ServerPree();
            server.SetApplication(new NetMsgCenter());     //设置应用层
            server.StartSever("127.0.0.1", 6666,100);
            DataBase.DatabaseManager.StartConnect();
            Console.ReadKey();

        }
    }
}
