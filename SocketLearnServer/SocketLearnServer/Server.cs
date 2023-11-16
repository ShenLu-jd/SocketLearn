using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLearnServer
{
    class Server
    {
        private int port = 7770;
        //private string host = "127.0.0.1";
        private string host = "172.16.60.105";
        private Socket socket;
        private Socket connectedSocket;

        public void CreateSocket()
        {
            // 获取本地 IP 地址
            string localIP = string.Empty;
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress adress in ipadrlist)
            {
                if (adress.AddressFamily == AddressFamily.InterNetwork)
                    localIP = adress.ToString();
            }
            host = localIP;
            // 创建 EndPoint
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);

            // 创建 socket 并开始监听
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipEndPoint); // 绑定 EndPoint 对象（port端口和ip地址）
            socket.Listen(1); // 开始监听, 设置最大连接数为1
            Console.WriteLine($"启动监听{socket.LocalEndPoint.ToString()}成功");

            // 监听 client 连接，只能同时与一个连接，后续连接将不再受理，直到服务器没有与其他客户端连接
            Task task = new Task(ListenClientConnect);
            task.Start();

            Console.WriteLine("当连接成功之后可以发送消息，如果希望断开连接输入 end");
            string? input = "start";
            while (input != "end")
            {
                input = Console.ReadLine();

                if (connectedSocket != null && connectedSocket.Connected)
                {
                    Send(input);
                }
                else
                {
                    Console.WriteLine("当前没有客户端连接，无法发送消息");
                }
            }
        }

        // 开始监听是否有客户端建立连接，只能与一个客户端同时连接
        private void ListenClientConnect()
        {
            while (true)
            {
                if(connectedSocket != null && connectedSocket.Connected)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    TryAccept();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message + " 建立连接错误！");
                    break;
                }
            }
        }

        // 等待一个客户端连接
        private void TryAccept()
        {
            connectedSocket = socket.Accept();
            Console.WriteLine("与客户端建立连接");
            Send("与服务端成功建立连接！");
            StartReceive();
        }

        // 与客户端建立连接后，监听客户端的消息
        private void StartReceive()
        {
            Task reveiveTask = new Task(() => ReceiveMessage(connectedSocket));
            reveiveTask.Start();
        }

        // 发送 string 类型的消息
        private bool Send(string msg)
        {
            if(connectedSocket != null && connectedSocket.Connected)
            {
                // msg 消息的字节数组
                byte[] buffer = Encoding.UTF8.GetBytes(msg);
                // msg 消息的字节数组长度的字节数组
                byte[] len = BitConverter.GetBytes(buffer.Length);
                // 要发送的字节数组
                byte[] content = new byte[len.Length + buffer.Length];

                Array.Copy(len, 0, content, 0, len.Length);
                Array.Copy(buffer, 0, content, len.Length, buffer.Length);

                try
                {
                    // 发送消息
                    connectedSocket.Send(content);
                    return true;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message + " 发送消息出错了！！！");
                }
            }
            return false;
        }

        // 接受消息
        private void ReceiveMessage(Socket clientSocket)
        {
            while(true)
            {
                if (clientSocket == null || !clientSocket.Connected) break;

                try
                {
                    // 接受 string 类型的消息
                    byte[] head = new byte[4];
                    clientSocket.Receive(head, head.Length, SocketFlags.None);
                    int len = BitConverter.ToInt32(head);

                    byte[] buffer = new byte[len];
                    clientSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("客户端消息：" + msg);
                }
                catch(Exception e)
                {
                    if(clientSocket != null)
                    {
                        clientSocket.Close();
                        clientSocket = null;
                    }
                    Console.WriteLine(e.Message + "\n 接受消息出错了！！！");
                    Console.WriteLine("已关闭客户端的 socket 连接");
                    break;
                }
            }
        }
    }
}
