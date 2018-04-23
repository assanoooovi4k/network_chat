using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        string UserName = String.Empty;
        protected internal TcpClient client;
        ServerObject server;
        bool NewMessage = false;
        bool isAuthorized = false;
        string NewMessageText = String.Empty;
        Thread CommandThread;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }
        public void Process()
        {
            try
            {
                CommandThread = new Thread(RecieveCommand);
                CommandThread.Start();
                while (!isAuthorized) { }
                string Message = UserName + " entered the chat";
                string header = "Command:" + "Connected";
                server.SendMessage(header, Id);
                header = "Command:" + "BroadCasting" + $"\r\nData:{Message}";
                server.BroadcastMessage(header, Id);
                Console.WriteLine(Message);
                while (true)
                {
                    if (NewMessage)
                    {
                        try
                        {
                            Message = $"{UserName}: {NewMessageText}";
                            Console.WriteLine(Message);
                            header = "Command:" + "BroadCasting" + $"\r\nData:{Message}";
                            server.BroadcastMessage(header, Id);
                            NewMessage = false;
                        }
                        catch
                        {

                        }
                    }
                    else
                        continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// Получение текущей команды от клиента
        /// </summary>
        private void RecieveCommand()
        {
            while(true)
            {
                byte[] rawdata = new byte[1024];
                string headerStr = String.Empty;
                client.Client.Receive(rawdata);
                headerStr = Encoding.ASCII.GetString(rawdata, 0, rawdata.Length);
                string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                Dictionary<string, string> headers = new Dictionary<string, string>();
                foreach (string s in splitted)
                {
                    if (s.Contains(":"))
                    {
                        headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                    }
                }
                if (headers.ContainsKey("Command"))
                {
                    string Command = headers["Command"].Trim('\0');
                    if (Command.Equals("TryAuth"))
                    {
                        if (Authorization.Authorize(headers["UserName"].Trim('\0'), headers["Password"].Trim('\0')))
                        {
                            server.SendMessage("Command:" + "SuccessAuth", Id);
                            UserName = headers["UserName"].Trim('\0');
                            isAuthorized = true;
                        }
                        else
                            server.SendMessage("Command:" + "FailedAuth", Id);
                    }
                    if (Command.Equals("Register"))
                    {
                        if (Authorization.Add(headers["UserName"].Trim('\0'), headers["Password"].Trim('\0')))
                        {
                            server.SendMessage("Command:" + "SuccessfullyRegistred", Id);
                            if (Authorization.Authorize(headers["UserName"].Trim('\0'), headers["Password"].Trim('\0')))
                            {
                                server.SendMessage("Command:" + "SuccessAuth", Id);
                                UserName = headers["UserName"].Trim('\0');
                                isAuthorized = true;
                            }
                            else
                                server.SendMessage("Command:" + "FailedAuth", Id);
                        }
                        else
                            server.SendMessage("Command:" + "AlreadyExists", Id);
                    }
                    if (Command.Equals("SendMessage"))
                    {
                        NewMessageText = headers["Message"].Trim('\0');
                        UserName = headers["UserName"].Trim('\0');
                        NewMessage = true;
                    }
                    if(Command.Equals("Disconnect"))
                    {
                        string Message = String.Empty;
                        if (!String.IsNullOrWhiteSpace(UserName))
                        {
                            Message = $"{UserName}: leaved the chat";
                            Console.WriteLine(Message);
                            string header = "Command:" + "BroadCasting" + $"\r\nData:{Message}";
                            server.BroadcastMessage(header, Id);
                            NewMessage = false;
                            isAuthorized = false;
                        }
                        server.RemoveConnection(Id);
                        Close();
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Разорвать соединение
        /// </summary>
        protected internal void Close()
        {
            if (client != null)
                client.Close();
        }
    }
}