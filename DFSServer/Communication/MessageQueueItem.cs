using DFSUtility;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DFSServer.Communication
{
    public class MessageQueueItem
    {
        public Socket Client { get; set; }
        public Request Request { get; set; }
    }
}
