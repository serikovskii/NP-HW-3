using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace NP_HW_3.Client
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private Thread listenThread;
        private Thread sendThread;
        private bool buttonIsEnabled;
        private string messageName;
        private ObservableCollection<string> namesUser;
        public MainWindow()
        {
            InitializeComponent();
            buttonIsEnabled = false;
            namesUser = new ObservableCollection<string>();
            namesUser.Add("All");
        }

        private void ConnectedButton(object sender, RoutedEventArgs e)
        {
            if (!buttonIsEnabled)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(IPAddress.Parse("127.0.0.1"), 12345);
                    listenThread = new Thread(LisetenThreadProc);
                    listenThread.Start(client);
                    sendThread = new Thread(SendThreadProc);
                    sendThread.Start(client);
                    buttonIsEnabled = true;
                    connect.Content = "Stop";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                sendThread.Abort();
                listenThread.Abort();
                client.Close();
                buttonIsEnabled = false;
                connect.Content = "Start";
            }
        }
        public void LisetenThreadProc(object obj)
        {
            var client = obj as TcpClient;
            byte[] buffer;

            while (true)
            {
                buffer = new byte[1024 * 4];
                var reciveSize = client.Client.Receive(buffer);
                var byteBuf = Encoding.UTF8.GetString(buffer, 5, reciveSize - 5);
                if (Encoding.UTF8.GetString(buffer, 0, reciveSize).Contains("name"))
                {
                    if (byteBuf != Dispatcher.Invoke(() => name.Text) && !namesUser.Contains(byteBuf))
                    {
                        Dispatcher.Invoke(() => namesUser.Add(byteBuf));
                        Dispatcher.Invoke(() => clients.ItemsSource = namesUser);
                    }
                    else
                        continue;
                }
                else
                    Dispatcher.Invoke(() => chat.AppendText($"{Encoding.UTF8.GetString(buffer, 0, reciveSize)} \n"));
                buffer = null;
            }
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                sendThread = new Thread(SendThreadProc);
                sendThread.Start(client);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendThreadProc(object obj)
        {
            var client = obj as TcpClient;
            string messageToServer = "";
            if (messageName == null)
            {
                Dispatcher.Invoke(() => messageName = name.Text);
                client.Client.Send(Encoding.GetEncoding(1251).GetBytes(messageName));
            }
            else if (Dispatcher.Invoke(() => clients.SelectedIndex>0))
            {
                Dispatcher.Invoke(() => messageName = $"#{clients.SelectedValue}*{message.Text}");
                client.Client.Send(Encoding.GetEncoding(1251).GetBytes(messageName));
            }
            else
            {
                Dispatcher.Invoke(() => messageToServer = message.Text);
                client.Client.Send(Encoding.GetEncoding(1251).GetBytes(messageToServer));
            }
        }
    }
}
