using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using DFSUtility;
using System.Collections.Generic;
using DFSServer.Services;
using DFSServer.Communication;
using System.IO;

namespace DFSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Select a root directory: ");
            //string rootDir = Console.ReadLine();
            string rootDir = "D:/server1";
            State.SetRootDirectory(rootDir);

            Console.Write("Total Space: ");
            State.LocalTotalSpace = 20000000;

            Task requestAcceptor = Task.Run(() => AcceptRequest());
            Task requestProcessor = Task.Run(() => ProcessRequestFromQueue());
            Task.WaitAll(new Task[] { requestAcceptor, requestProcessor });
        }

        static void AcceptRequest()
        {
            RequestListener listener = new RequestListener();
            listener.Listen(100);

            while (true)
            {
                Console.WriteLine("Waiting");
                //this is blocking
                listener.Accept();
            }
        }

        static void ProcessRequestFromQueue()
        {
            while (true)
            {
                var request = MessageQueue.Dequeue();
                if (request != null)
                {
                    var executingAssembly = Assembly.GetExecutingAssembly();
                    Type type = executingAssembly.GetTypes()
                        .FirstOrDefault(x => x.FullName.Contains(request.Request.Type));
                    var met = type.GetMethod(request.Request.Method, BindingFlags.Static | BindingFlags.Public);

                    //parameters = null when not sent so it's fine
                    object value = met.Invoke(null, request.Request.Parameters);

                    var response = (Response) value;
                    response.Request = request.Request;

                    byte[] buffer = response.SerializeToByteArray();

                    //Console.WriteLine($"size of response: {buffer.Length}");
                    int bytesSent = 0, totalBytesSent = 0;
                    do
                    {
                        bytesSent = request.Client.Send(buffer);
                        totalBytesSent += bytesSent;
                        //Console.WriteLine($"total bytes sent till now: { totalBytesSent }, iteration: {bytesSent}");
                    }
                    while (totalBytesSent < buffer.Length);
                }
            }
        }
    }
}
