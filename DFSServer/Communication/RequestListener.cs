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
            ipAddress = IPAddress.Parse("192.168.0.105");
            int port = 11000;
            while (IsPortOccupied(port))
                port++;
            localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a Socket that will use Tcp protocol      
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);

            State.LocalEndPoint = localEndPoint;
            State.Listener = listener;

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
                try
                {
                    var bytes = Network.ReceiveResponse(client, 100000);
                    var request = bytes.Deserialize<Request>();

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
