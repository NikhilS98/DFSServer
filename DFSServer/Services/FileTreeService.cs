﻿using DFSUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Services
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
            ServerCommunication.Broadcast(response.SerializeToByteArray());
        }
    }
}
