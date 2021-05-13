using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    public class ServerPree
    {
        private Socket serverSocket;                  //服务器Socket
        private Semaphore semaphore;                  //计量器
        private ClientPeerPool clientPeerPool;        //客户端对象连接池
        /// <summary>
        /// 应用层
        /// </summary>
        private IApplication application;
        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="application"></param>
        public void SetApplication(IApplication application)
        {
            this.application = application;
        }
        ///<summary>
        ///开启服务器
        ///</summary>
        public void StartSever(string ip, int port, int maxClient)
        {
            try
            {
                clientPeerPool = new ClientPeerPool(maxClient);
                semaphore = new Semaphore(maxClient, maxClient);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //赋初始值
                //填满客户端对象连接池
                for (int i = 0; i < maxClient; i++)
                {
                    ClientPeer temp = new ClientPeer();
                    temp.receiveCompletted = ReceiveProcessCompleted;
                    temp.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    clientPeerPool.Enqueue(temp);
                }

                //绑定到进程
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //最大的监听数
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");

                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        #region 接收客户端的连接请求
        /// <summary>
        /// 接收客户端的连接
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }

            //result为true的话 代表正在接收连接，连接成功后 触发e.Completed事件
            //result为false的话 代表接收成功
            bool result = serverSocket.AcceptAsync(e);
            if (result == false)
            {
                ProcessAccept(e);
            }

        }

        /// <summary>
        /// 异步接收客户端的连接完成后触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }


        /// <summary>
        /// 处理连接请求
        /// </summary>

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            semaphore.WaitOne();  //阻止当前线程 有信号在连接
            ClientPeer client = clientPeerPool.Dequeue();
            client.clientSocket = e.AcceptSocket;
            Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端连接成功");
            //接收消息TODO
            StartReceive(client);
            e.AcceptSocket = null;
            StartAccept(e);  //循环接收客户端连接
        }
        #endregion
        #region 接收数据
        /// <summary>
        /// 开始接收数据
        /// </summary>
        private void StartReceive(ClientPeer client)
        {
            try
            {
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 异步接收数据完成后的调用
        /// </summary>
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }
        /// <summary>
        /// 处理数据的接收
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            ClientPeer client = e.UserToken as ClientPeer;
            //判断数据接收的时候是不是成功的 数据是不是为0的
            //为0表示断开连接
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);


                //让ClientPeer自身处理接收到的数据
                client.ProcesRecevie(packet);
                StartReceive(client);
            }
            //断开连接
            else
            {
                //没有传输的字节数，就代表断开连接了
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnect(client, "客户端主动断开链接");
                    }
                    //因为网络异常被动断开连接
                    else
                    {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// 一条消息处理完成后的回调
        /// </summary>
        private void ReceiveProcessCompleted(ClientPeer client, NetMsg msg)
        {
            //交给应用层处理这个消息
            application.Receive(client, msg);
        }
        #endregion
        #region 断开连接
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        private void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空，无法断开连接");
                }
                Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端断开连接，原因" + reason);
                application.Disconnect(client);
                //让客户端处理断开连接
                client.Disconnect();
                clientPeerPool.Enqueue(client);
                semaphore.Release();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
