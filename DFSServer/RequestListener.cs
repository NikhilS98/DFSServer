using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DFSUtility;

namespace DFSServer
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
            localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a Socket that will use Tcp protocol      
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method  
            listener.Bind(localEndPoint);
               
        }

        public void Listen(int requestBacklog)
        {
            // Specify how many requests a Socket can listen before it gives Server busy response.  
            listener.Listen(requestBacklog);
        }

        public void Accept()
        {
            Socket client = listener.Accept();
            client.Send(Encoding.UTF8.GetBytes("Connected"));
            ClientList.Add(client);
            Task.Run(() => ListenRequest(client));
        }

        private void ListenRequest(Socket client)
        {
            while (true)
            {
                Console.WriteLine("Listening on client " + client.RemoteEndPoint);
                List<byte> bytesList = new List<byte>();
                int size = 100;

                byte[] buffer = new byte[size];
                int bytesTransferred = 0;
                do
                {
                    //Add in try catch. if client disconnects remove from list
                    bytesTransferred = client.Receive(buffer);
                    for(int i = 0; i < bytesTransferred; i++)
                    {
                        bytesList.Add(buffer[i]);
                    }
                }
                while (bytesTransferred == size);

                var request = bytesList.ToArray().Deserialize<Request>();

                AddRequestInQueue(client, request);

                Console.WriteLine(request);

                string msg = "Received";
                byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
                client.Send(msgBytes);
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
    }
}
