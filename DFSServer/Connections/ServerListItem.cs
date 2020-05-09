using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DFSServer.Connections
{
    public class ServerListItem
    {
        public ServerListItem(Socket socket, string ipPort)
        {
            Socket = socket;
            IPPort = ipPort;
        }

        public Socket Socket { get; set; }
        public string IPPort { get; set; }
    }
}
