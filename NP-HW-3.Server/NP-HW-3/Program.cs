using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NP_HW_3
{
    class Program
    {
        static TcpListener server;
        static Thread thread;
        static ManualResetEvent eventStop;
        static Dictionary<string, TcpClient> clients;


        static void Main(string[] args)
        {
            try
            {
                eventStop = new ManualResetEvent(false);
                server = new TcpListener(IPAddress.Any, 12345);
                server.Start();
                Console.WriteLine("Ожидани подключения");
                thread = new Thread(ServerThreadProcedure);
                thread.Start(server);
                clients = new Dictionary<string, TcpClient>();
            }
            catch (Exception exctption)
            {
                Console.WriteLine(exctption.Message, "Error");
            }
        }
        static void ServerThreadProcedure(object obj)
        {
            TcpListener server = (TcpListener)obj;

            while (true)
            {
                IAsyncResult asyncResult = server.BeginAcceptSocket(AsyncServerProc, server);
                while (asyncResult.AsyncWaitHandle.WaitOne(200) == false)
                {
                    if (eventStop.WaitOne(0) == true)
                        return;
                }

            }
        }

        static void AsyncServerProc(IAsyncResult iAsync)
        {

            TcpListener server = (TcpListener)iAsync.AsyncState;
            TcpClient client = server.EndAcceptTcpClient(iAsync);
            Console.WriteLine("Подключился клиент");
            Console.WriteLine("IP адрес клиента " + client.Client.RemoteEndPoint.ToString() + "\n");
            ThreadPool.QueueUserWorkItem(ClientThreadProc, client);

        }

        static void ClientThreadProc(object obj)
        {
            TcpClient client = (TcpClient)obj;

            Console.WriteLine("Рабочий поток клиента запущен");
            var buffer = new byte[1024 * 4];

            string clientName;
            string messageClient = "";
            string messageServer = "";
            int reciveSize;
            int reciveSizeName;
            List<string> names = new List<string>();
            reciveSizeName = client.Client.Receive(buffer);
            clientName = Encoding.GetEncoding(1251).GetString(buffer, 0, reciveSizeName);
            clients.Add(clientName, client);
            Console.WriteLine($"Клиент {clientName} \r\n");

            
            foreach(var cl in clients)
            {
                names.Add(cl.Key);
                //clients.ElementAt(i).Value.Client.Send(Encoding.GetEncoding(1251).GetBytes($"name {clients.ElementAt(j).Key}"));

            }
            
            for (int i = 0; i < clients.Count(); i++)
            {
                clients.ElementAt(i).Value.Client.Send(Encoding.GetEncoding(1251).GetBytes($"New user connected {clientName}#{names.ToString()}"));
               
            }



            while (true)
            {

                var ipClient = client.Client.RemoteEndPoint.ToString();
                reciveSize = client.Client.Receive(buffer);
                if (reciveSize == 0)
                {
                    Console.WriteLine("stop");
                    clients.Clear();
                    break;
                }
                messageClient = Encoding.GetEncoding(1251).GetString(buffer, 0, reciveSize);
                messageServer = $"{clientName}: {messageClient} \n";

                for (int i = 0; i < clients.Count(); i++)
                {
                    clients.ElementAt(i).Value.Client.Send(Encoding.GetEncoding(1251).GetBytes(messageServer));
                }



            }
        }
    }
}
