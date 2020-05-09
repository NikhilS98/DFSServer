using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace DFSServer.Connections
{
    public static class ServerList
    {
        private static List<ServerListItem> serverList = new List<ServerListItem>();
        public static int Capacity { get; set; } = 10;
        private static readonly object updateLock = new object();

        public static bool Add(Socket server, string ipPort)
        {
            if (HasSpace())
            {
                lock (updateLock)
                {
                    serverList.Add(new ServerListItem(server, ipPort));
                }
                return true;
            }
            return false;
        }

        public static ServerListItem Remove(Socket socket)
        {
            var item = serverList.FirstOrDefault(x => x.Socket.Equals(socket));
            if(item != null)
            {
                lock (updateLock)
                {
                    serverList.Remove(item);
                }
            }
            return item;
        }

        public static ServerListItem Remove(string ipPort)
        {
            var item = serverList.FirstOrDefault(x => x.IPPort.Equals(ipPort));
            if (item != null)
            {
                lock (updateLock)
                {
                    serverList.Remove(item);
                }
            }
            return item;
        }

        public static int GetCount()
        {
            return serverList.Count;
        }

        public static bool HasSpace()
        {
            return GetCount() < Capacity;
        }

        public static List<Socket> GetServers()
        {
            List<Socket> sockets = new List<Socket>();
            foreach (var item in serverList)
            {
                sockets.Add(item.Socket);
            }
            return sockets;
        }

        public static List<string> GetIPPorts()
        {
            List<string> ipPorts = new List<string>();
            foreach (var item in serverList)
            {
                ipPorts.Add(item.IPPort);
            }
            return ipPorts;
        }

        public static List<ServerListItem> GetServerList()
        {
            return serverList;
        }

    }
}
