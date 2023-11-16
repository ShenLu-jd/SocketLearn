using System;

namespace SocketLearnServer
{
    class Program
    {

        static void Main(string[] args)
        {
            Server s = new Server();
            s.CreateSocket();
        }
    }
}
