using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public class ClientPeer
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public Socket clientSocket { get; set; }
        private NetMsg msg;  //网络消息类

        //赋初始值 给异步的套接字
        public ClientPeer()
        {
            msg = new NetMsg();
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this;
            //设置数据缓冲区
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }
        #region 接收数据
        /// <summary>
        /// 接收异步套接字操作
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }
        /// <summary>
        /// 接收到消息之后，存放到数据缓冲区
        /// </summary>
        private List<byte> cache = new List<byte>();
        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isProcessingReceice = false;
        /// <summary>
        /// 消息处理完的委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public delegate void ReceiveCompletted(ClientPeer client, NetMsg msg);
        public ReceiveCompletted receiveCompletted;
        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="packet"></param>
        public void ProcesRecevie(byte[] packet)
        {
            //添加集合数据到末尾
            cache.AddRange(packet);
            if (isProcessingReceice == false)
                ProcessData();
        }
        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            isProcessingReceice = true;
            byte[] packet = EncodeTool.DecodePacket(ref cache);
            if (packet == null)
            {
                isProcessingReceice = false;
                return;
            }
            NetMsg msg = EncodeTool.DecodeMsg(packet);   //回调给服务器
            if (receiveCompletted != null)
            {
                receiveCompletted(this, msg);
            }
            ProcessData();
        }
        #endregion

        #region 发送消息
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="value">参数</param>
        public void SendMsg(int opCode, int subCode, object value)
        {
            msg.Change(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            SendMsg(packet);
        }
        public void SendMsg(byte[] packet)
        {
            //字节数组
            try
            {
                clientSocket.Send(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 断开连接
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            cache.Clear();
            isProcessingReceice = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket = null;
        }
        #endregion

    }
}
