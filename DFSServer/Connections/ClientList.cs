using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DFSServer.Connections
{
    public static class ClientList
    {
        private static ConcurrentDictionary<EndPoint, Socket> clients = 
            new ConcurrentDictionary<EndPoint, Socket>();
        public static int Capacity { get; set; } = 10;

        public static bool Add(Socket client)
        {
            if(HasSpace())
                return clients.TryAdd(client.RemoteEndPoint, client);
            return false;
        }

        public static Socket Remove(EndPoint remoteEndPoint)
        {
            Socket socket;
            clients.TryRemove(remoteEndPoint, out socket);
            return socket;
        }

        public static int GetCount()
        {
            return clients.Count;
        }

        public static bool HasSpace()
        {
            return GetCount() < Capacity;
        }

        public static List<Socket> GetClientSockets()
        {
            List<Socket> sockets = new List<Socket>();
            foreach (var item in clients)
            {
                sockets.Add(item.Value);
            }
            return sockets;
        }
    }
}
