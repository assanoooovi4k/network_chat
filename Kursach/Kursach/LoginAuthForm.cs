using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Kursach
{
    public partial class LoginAuthForm : Form
    {
        string headerStr;
        Listener listener;
        bool exitwithoutpacket = false;
        byte[] buffer = new byte[1024];
        public LoginAuthForm()
        {
            InitializeComponent();
        }
        private void LoginAuthForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (!exitwithoutpacket)
                listener.client.Client.Send(Encoding.ASCII.GetBytes($"Command:Disconnect"));
        }
        private void LoginAuthForm_Load(object sender, EventArgs e)
        {
            try
            {
                listener = Listener.getInstance();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                exitwithoutpacket = true;
                Application.Exit();
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            headerStr = "Command:TryAuth" + $"\r\nUserName:{textBoxLogin.Text}" + $"\r\nPassword:{textBoxPass.Text}";
            listener.client.Client.Send(Encoding.ASCII.GetBytes(headerStr));
            listener.client.Client.Receive(buffer);
            headerStr = Encoding.ASCII.GetString(buffer);
            string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (string s in splitted)
            {
                if (s.Contains(":"))
                {
                    headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                }
            }
            string Command = headers["Command"].Trim('\0');
            if (headers.ContainsKey("Command"))
            {
                if (Command.Equals("SuccessAuth"))
                {
                    MessageBox.Show($"Авторизация прошла успешно", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    buttonLogin.Enabled = false;
                    buttonRegister.Enabled = false;
                    ChatForm ChatForm = new ChatForm(textBoxLogin.Text);
                    Hide();
                    ChatForm.Show();
                }
                if (Command.Equals("FailedAuth"))
                {
                    MessageBox.Show($"Введен неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxPass.Text = "";
                }
            }
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            headerStr = "Command:Register" + $"\r\nUserName:{textBoxLogin.Text}" + $"\r\nPassword:{textBoxPass.Text}";
            listener.client.Client.Send(Encoding.ASCII.GetBytes(headerStr));
            listener.client.Client.Receive(buffer);
            headerStr = Encoding.ASCII.GetString(buffer);
            string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (string s in splitted)
            {
                if (s.Contains(":"))
                {
                    headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                }
            }
            string Command = headers["Command"].Trim('\0');
            if (headers.ContainsKey("Command"))
            {
                if (Command.Equals("SuccessfullyRegistred"))
                {
                    MessageBox.Show($"Регистрация прошла успешно", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    buttonLogin.Enabled = false;
                    buttonRegister.Enabled = false;
                    ChatForm ChatForm = new ChatForm(textBoxLogin.Text);
                    Hide();
                    ChatForm.Show();
                }
                if (Command.Equals("AlreadyExists"))
                {
                    MessageBox.Show($"Пользователь с таким именем уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxLogin.Text = "";
                    textBoxPass.Text = "";
                }
            }
        }
    }
}
