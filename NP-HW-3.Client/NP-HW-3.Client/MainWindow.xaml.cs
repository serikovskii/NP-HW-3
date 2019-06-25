using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NP_HW_3.Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private Thread listenThread;
        private Thread sendThread;
        private bool buttonIsEnabled;
        private string messageName;
        private List<string> namesUser;
        public MainWindow()
        {
            InitializeComponent();
            buttonIsEnabled = false;
            namesUser = new List<string>();
        }

        private void ConnectedButton(object sender, RoutedEventArgs e)
        {
            if (!buttonIsEnabled)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(IPAddress.Parse(ip.Text), int.Parse(port.Text));
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
            var clietnsName = new List<string>();
            while (true)
            {
                var buffer = new byte[1024 * 4];
                var reciveSize = client.Client.Receive(buffer);
                var reciveBuffer = Encoding.UTF8.GetString(buffer, 0, reciveSize);
                int bufferSize = 0;
                string nameUser;
                for (int i = 0; i < reciveSize; i++)
                {
                    if(reciveBuffer[i] == '#')
                    {
                        bufferSize = i;
                        break;
                    }
                }
                Dispatcher.Invoke(() => chat.AppendText($"{Encoding.UTF8.GetString(buffer, 0, reciveSize-bufferSize)} \n"));
                nameUser = Encoding.UTF8.GetString(buffer, bufferSize + 1, reciveSize);
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
            else
            {
                Dispatcher.Invoke(() => messageToServer = message.Text);
                client.Client.Send(Encoding.GetEncoding(1251).GetBytes(messageToServer));

            }

        }
    }
}
