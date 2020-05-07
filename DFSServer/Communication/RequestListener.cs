using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DFSUtility;
using System.Linq;
using DFSServer.Connections;

namespace DFSServer.Communication
{
    public class RequestListener
    {
        private IPHostEntry host;
        private IPAddress ipAddress;
        private IPEndPoint localEndPoint;
        private Socket listener;

        public RequestListener()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            host = Dns.GetHostEntry("localhost");
            ipAddress = host.AddressList[0];
            int port = 11000;
            while (IsPortOccupied(port))
                port++;
            localEndPoint = new IPEndPoint(ipAddress, port);

            State.LocalEndPoint = localEndPoint;

            // Create a Socket that will use Tcp protocol      
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method  
            listener.Bind(localEndPoint);

            Console.WriteLine($"Running on {localEndPoint}");

        }

        public void Listen(int requestBacklog)
        {
            // Specify how many requests a Socket can listen before it gives Server busy response.  
            listener.Listen(requestBacklog);
        }

        public void Accept()
        {
            Socket client = listener.Accept();
            client.Send(Encoding.UTF8.GetBytes("root/"));
            ClientList.Add(client);
            Console.WriteLine(client.RemoteEndPoint.ToString());
            Task.Run(() => ListenRequest(client));
        }

        private void ListenRequest(Socket client)
        {
            while (true)
            {
                Console.WriteLine("Listening on client " + client.RemoteEndPoint);
                List<byte> bytesList = new List<byte>();
                int size = 100;

                try
                {
                    byte[] buffer = new byte[size];
                    int bytesTransferred = 0;
                    do
                    {
                        bytesTransferred = client.Receive(buffer);
                        for (int i = 0; i < bytesTransferred; i++)
                        {
                            bytesList.Add(buffer[i]);
                        }

                    }
                    while (bytesTransferred == size);

                    var request = bytesList.ToArray().Deserialize<Request>();

                    AddRequestInQueue(client, request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    ClientList.Remove(client.RemoteEndPoint);
                    break;
                }
            }
        }

        private void AddRequestInQueue(Socket client, Request request)
        {
            var requestItem = new MessageQueueItem
            {
                Client = client,
                Request = request
            };
            MessageQueue.Enqueue(requestItem);
        }

        private bool IsPortOccupied(int port)
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            bool isOccupied = tcpConnInfoArray.Any(x => x.Port == port);

            return isOccupied;
        }
    }
}
