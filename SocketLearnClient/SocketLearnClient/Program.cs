using System;

namespace SocketLearnClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client();
            c.CreateSocket();
        }
    }
}
