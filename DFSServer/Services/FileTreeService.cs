using DFSUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Communication
{
    public static class FileTreeService
    {
        public static void UpdateFileTree(byte[] data)
        {
            Response response  = new Response
            {
                Command = Command.updateFileTree,
                Bytes = data
            };
            ServerCommunication.BroadcastToServers(response.SerializeToByteArray());
        }
    }
}
