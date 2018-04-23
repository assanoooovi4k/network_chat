using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ServerObject
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();

        /// <summary>
        /// Добавить соеденение
        /// </summary>
        /// <param name="clientObject">Объект клиента</param>
        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        /// <summary>
        /// Разорвать соеденение от определенного пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(x => x.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        /// <summary>
        /// Начать слушать сокет
        /// </summary>
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                tcpListener.Start();
                Console.WriteLine("Server started. Waiting for connections...");
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine($"Incoming connection from {tcpClient.Client.RemoteEndPoint}");
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        /// <summary>Отправить сообщение всем пользователям</summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="id">ID пользователя</param>
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].client.Client.Send(data);
            }
        }
        /// <summary>Отправить сообщение определенному пользователю</summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="id">ID пользователя</param>
        protected internal void SendMessage(string message, string id)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    clients[i].client.Client.Send(data);
                    break;
                }
            }
        }
        /// <summary>
        /// Отключить всех пользователей
        /// </summary>
        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            foreach(var client in clients)
            {
                client.Close(); 
            }
            Environment.Exit(0); 
        }
    }
}