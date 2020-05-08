using DFSServer.Services;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DFSServer.Communication
{
    public static class RequestProcessor
    {
        public static void ProcessRequestFromQueue()
        {
            while (true)
            {
                var request = MessageQueue.Dequeue();
                if (request != null)
                {
                    byte[] buffer = null;

                    if (request.Request.Command == Command.serverConnect)
                    {
                        var ips = ServerCommunication.AcceptConnection(request.Request, request.Client);
                        if (ips == null)
                            buffer = Encoding.UTF8.GetBytes("Rejected");
                        else
                            buffer = ips.SerializeToByteArray();
                    }
                    else if(request.Request.Command == Command.requestFileTree)
                    {
                        var rootDirNode = FileTree.GetRootDirectory();
                        var response = new Response
                        {
                            Bytes = rootDirNode.SerializeToByteArray(),
                            IsSuccess = true,
                            Request = request.Request,
                        };
                        buffer = response.SerializeToByteArray();
                    }
                    else
                    {
                        var executingAssembly = Assembly.GetExecutingAssembly();
                        Type type = executingAssembly.GetTypes()
                            .FirstOrDefault(x => x.FullName.Contains(request.Request.Type));
                        var met = type.GetMethod(request.Request.Method, BindingFlags.Static | BindingFlags.Public);

                        //parameters = null when not sent so it's fine
                        object value = met.Invoke(null, request.Request.Parameters);

                        var response = (Response)value;
                        response.Request = request.Request;

                        buffer = response.SerializeToByteArray();

                    }

                    //Thread.Sleep(2000);
                    //Console.WriteLine($"size of response: {buffer.Length}");
                    Network.SendRequest(request.Client, buffer);
                }
            }
        }
    }
}
