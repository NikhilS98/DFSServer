using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using DFSUtility;
using System.Collections.Generic;
using DFSServer.Communication;
using System.IO;
using DFSServer.Connections;
using DFSServer.Helpers;
using DFSServer.Services;

namespace DFSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Select a root directory: ");
            string rootDir = Console.ReadLine();
            //string rootDir = "D:\\server1";
            State.SetRootDirectory(rootDir);

            Console.Write("Total Space: ");
            State.LocalTotalSpace = 20000000;

            //Console.Write("Port: ");
            //string port = Console.ReadLine();

            RequestListener listener = new RequestListener();
            listener.Listen(100);

            var ips = File.ReadAllLines(CommonFilePaths.ConfigFile);
            var ip = ips.FirstOrDefault(x => !x.Equals(State.LocalEndPoint.ToString()));
            if (ip != null)
            {
                List<string> ipList = ServerCommunication.Connect(ip);
                File.WriteAllLines(CommonFilePaths.ConfigFile, ipList);
                foreach (var item in ipList)
                {
                    if (!item.Equals(State.LocalEndPoint.ToString()) && !item.Equals(ip))
                        ServerCommunication.Connect(item);
                }
                var servers = ServerList.GetServers();
                if (servers != null)
                {
                    var r = new Request { Command = Command.requestFileTree };
                    Network.SendRequest(servers[0], r.SerializeToByteArray());
                }
            }

            Task requestAcceptor = Task.Run(() => AcceptRequest(listener));
            Task requestProcessor = Task.Run(() => RequestProcessor.ProcessRequestFromQueue());
            Task.WaitAll(new Task[] { requestAcceptor, requestProcessor });
        }

        static void AcceptRequest(RequestListener listener)
        {
            while (true)
            {
                Console.WriteLine("Waiting");
                //this is blocking
                listener.Accept();
            }
        }
    }
}
