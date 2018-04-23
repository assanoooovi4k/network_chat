using System.Net.Sockets;

namespace Kursach
{
    class Listener
    {
        private static Listener instance;
        public TcpClient client = new TcpClient();
        private Listener()
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 8888);
        }

        public static Listener getInstance()
        {
            if (instance == null)
                instance = new Listener();
            return instance;
        }
    }
}
