using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketLearnClient
{
    class Client
    {
        private int port = 7770;    // 服务器端口
        private string host = "127.0.0.1";  // 服务器IP
        private Socket socket;
        private Socket connectedSocket;

        public void CreateSocket()
        {
            Console.WriteLine("请输入服务器IP地址（直接回车则默认使用127.0.0.1）：");
            string inputHost = Console.ReadLine();
            //inputHost = "172.16.60.105";
            host = string.IsNullOrEmpty(inputHost) ? host : inputHost;
            Console.WriteLine("IP:" + host);

            // 创建 EndPoint
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);

            // 创建 socket 并开始监听
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipEndPoint); // 服务器的IP和端口
                Console.WriteLine("连接服务器成功！");
                connectedSocket = socket;
            }
            catch(Exception e)
            {
                Console.WriteLine("连接服务器失败！");
                return;
            }

            // 接收 client 连接
            Task task = new Task(StartReceive);
            task.Start();

            Console.WriteLine("输入消息进行发送，如果希望断开连接输入 end");
            string? input = "start";
            while (input != "end")
            {
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                if (connectedSocket != null && connectedSocket.Connected)
                {
                    Send(input);
                }
                else
                {
                    Console.WriteLine("与服务器断开连接，无法发送消息");
                }
            }
        }

        private void ListenClientConnect()
        {
            while (true)
            {
                connectedSocket = socket.Accept();
                Console.WriteLine("与客户端建立连接");
                Send("与服务端成功建立连接！");
                StartReceive();
            }
        }

        private void StartReceive()
        {
            Task reveiveTask = new Task(() => ReceiveMessage(connectedSocket));
            reveiveTask.Start();
        }

        // 发送 string 类型的消息
        private bool Send(string msg)
        {
            if (connectedSocket != null && connectedSocket.Connected)
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
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " 发送消息出错了！！！");
                }
            }
            return false;
        }

        // 接受消息
        private void ReceiveMessage(Socket serverSocket)
        {
            while (true)
            {
                if (serverSocket == null) break;

                try
                {
                    // 接受 string 类型的消息
                    byte[] head = new byte[4];
                    serverSocket.Receive(head, head.Length, SocketFlags.None);
                    int len = BitConverter.ToInt32(head);

                    byte[] buffer = new byte[len];
                    serverSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("服务端消息：" + msg);
                }
                catch (Exception e)
                {
                    connectedSocket.Close();
                    Console.WriteLine(e.Message + " 接受消息出错了！！！");
                    Console.WriteLine("socket 已关闭");
                    break;
                }
            }
        }
    }
}
