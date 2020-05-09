using DFSServer.Connections;
using DFSServer.Services;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DFSServer.Communication
{
    public static class RequestProcessor
    {
        public static void DequeueRequestFromQueue()
        {
            while (true)
            {
                var queueItem = MessageQueue.Dequeue();
                if (queueItem != null)
                {
                    Task.Run(() => ProcessRequest(queueItem));
                }
            }
        }

        private static void ProcessRequest(MessageQueueItem queueItem)
        {
            byte[] buffer = null;

            if (queueItem.Request.Command == Command.requestFileTree)
            {
                var rootDirNode = FileTree.GetRootDirectory();
                var response = new Response
                {
                    Bytes = rootDirNode.SerializeToByteArray(),
                    IsSuccess = true,
                    Command = Command.updateFileTree
                };
                buffer = response.SerializeToByteArray();
            }
            else
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                Type type = executingAssembly.GetTypes()
                    .FirstOrDefault(x => x.FullName.Contains(queueItem.Request.Type));
                var met = type.GetMethod(queueItem.Request.Method, BindingFlags.Static | BindingFlags.Public);

                //parameters = null when not sent so it's fine
                object value = met.Invoke(null, queueItem.Request.Parameters);

                var response = (Response)value;

                if (response.Command == Command.forwarded)
                {
                    Socket server = ServerList.GetServerList()
                        .FirstOrDefault(x => x.IPPort.Equals(response.Message)).Socket;
                    queueItem.Request.Guid = Guid.NewGuid();
                    Network.Send(server, queueItem.Request.SerializeToByteArray());
                    while (!ServerResponseList.ContainsKey(queueItem.Request.Guid))
                    {

                    }
                    response = ServerResponseList.Remove(queueItem.Request.Guid);
                }
                response.Request = queueItem.Request;

                buffer = response.SerializeToByteArray();

            }

            //Thread.Sleep(2000);
            //Console.WriteLine($"size of response: {buffer.Length}");
            Network.Send(queueItem.Client, buffer);
        }
    }
}
