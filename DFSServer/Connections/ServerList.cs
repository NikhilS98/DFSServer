using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DFSServer.Connections
{
    public static class ServerList
    {
        private static ConcurrentDictionary<EndPoint, Socket> servers =
            new ConcurrentDictionary<EndPoint, Socket>();
        public static int Capacity { get; set; } = 10;

        public static bool Add(Socket client)
        {
            if (HasSpace())
                return servers.TryAdd(client.RemoteEndPoint, client);
            return false;
        }

        public static Socket Remove(EndPoint remoteEndPoint)
        {
            Socket socket;
            servers.TryRemove(remoteEndPoint, out socket);
            return socket;
        }

        public static int GetCount()
        {
            return servers.Count;
        }

        public static bool HasSpace()
        {
            return GetCount() < Capacity;
        }

        public static ConcurrentDictionary<EndPoint, Socket> GetServerDictionary()
        {
            return servers;
        }

        public static List<Socket> GetServers()
        {
            List<Socket> sockets = new List<Socket>();
            foreach (var item in servers)
            {
                sockets.Add(item.Value);
            }
            return sockets;
        }

    }
}
