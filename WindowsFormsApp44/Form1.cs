using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp44
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private string username;
        private DateTime ConnectionStartTime;
        public Form1()
        {
            InitializeComponent();
        }
        private void AuthenticateUser()
        {
            // Open authentication form
            using (var loginForm = new LoginForm())
            {
                loginForm.ShowDialog();

                // Get entered username and password
                username = loginForm.Username;
                string password = loginForm.Password;

                // Send username and password to the server for authentication or registration
                SendMessage($"AUTH|{username}|{password}");
            }
        }

        private void SendInformationThread()
        {
            ConnectionStartTime = DateTime.Now;

            while (true)
            {
                // Send information to the server every minute
                SendMessage($"Client: {username}, Time: {DateTime.Now}, Connection Time: {DateTime.Now - ConnectionStartTime}");
                Thread.Sleep(60000); // Sleep for 1 minute
            }
        }

        private void ReceiveMessagesThread()
        {
            while (true)
            {
                // Receive and process messages from the server
                string message = ReceiveMessage();
                if (message == null)
                    break;

                // Handle received messages, update UI, etc.
                // For now, let's just append the message to a TextBox named "chatTextBox"
                AppendToChat(message);
            }
        }

        private void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        private string ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                return null;

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private void AppendToChat(string message)
        {
            // This is a simple way to update the UI from a different thread
            if (chatRichTextBox.InvokeRequired)
            {
                chatRichTextBox.Invoke(new Action(() => AppendToChat(message)));
            }
            else
            {
                chatRichTextBox.AppendText(message + Environment.NewLine);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the connection when the form is closing
            if (client != null && client.Connected)
            {
                client.Close();
            }
        }

        private void connectButton_Click_1(object sender, EventArgs e)
        {
            client = new TcpClient();

            try
            {
                client.Connect("127.0.0.1", 8888);
                stream = client.GetStream();

                // Authenticate user or register if not registered
                AuthenticateUser();

                // Start a thread to send information every minute
                Thread sendInfoThread = new Thread(SendInformationThread);
                sendInfoThread.Start();

                // Start a thread to receive messages
                Thread receiveThread = new Thread(ReceiveMessagesThread);
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to the server: " + ex.Message);
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            // Отправляем сообщение на сервер
            SendMessage(messageTextBox.Text);
            // Очищаем поле ввода после отправки
            messageTextBox.Text = string.Empty;
        }
    }
}
