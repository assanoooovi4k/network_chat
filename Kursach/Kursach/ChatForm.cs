using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursach
{
    public partial class ChatForm : Form
    {
        delegate void TextBoxTextDelegate(string text);
        Listener listener;
        string UserName;
        public ChatForm(string UserName)
        {
            InitializeComponent();
            this.UserName = UserName;
            labelUserName.Text += UserName;
            listener = Listener.getInstance();
            Thread ListenThread = new Thread(Listen);
            ListenThread.Start();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            listener.client.Client.Send(Encoding.ASCII.GetBytes($"Command:SendMessage\r\nMessage:{textBoxMessage.Text}\r\nUserName:{UserName}"));
        }
        public void Listen()
        {
            while (true)
            {
                byte[] rawdata = new byte[1024];
                string headerStr = String.Empty;
                listener.client.Client.Receive(rawdata);
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
                    if (Command.Equals("BroadCasting"))
                    {
                        TextBoxText(headers["Data"].Trim('\0') + "\r\n");
                    }
                    if (Command.Equals("Connected"))
                    {
                        TextBoxText("Вы были подключены к чату\r\n");
                    }
                }
            }
        }
        private void TextBoxText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TextBoxTextDelegate(TextBoxText), new object[] { text });
                return;
            }
            else
            {
                textBoxChat.Text+=text;
            }
        }
        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
