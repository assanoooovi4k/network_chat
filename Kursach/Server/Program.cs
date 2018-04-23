using System;
using System.Threading;

namespace Server
{
    class Program
    {
        static ServerObject server;
        static Thread listenThread;
        static void Main(string[] args)
        {
            try
            {
                Authorization.GetData();
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
