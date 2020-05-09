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

namespace DFSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("Select a root directory: ");
            //string rootDir = Console.ReadLine();
            string rootDir = "D:\\server";
            State.SetRootDirectory(rootDir);

            RequestListener listener = new RequestListener();
            listener.Listen(100);

            if(!File.Exists(CommonFilePaths.ConfigFile))
                File.WriteAllText(CommonFilePaths.ConfigFile, "192.168.99.1:11000\n");
            //CommonFilePaths.ConfigFile = rootDir + "\\config.txt";

            FileTree.ReadFromFile();

            var ips = File.ReadAllLines(CommonFilePaths.ConfigFile);
            if (ips != null)
            {
                foreach (var ip in ips)
                {
                    if (!string.IsNullOrEmpty(ip) && !ip.Equals(State.LocalEndPoint.ToString()))
                    {
                        try
                        {
                            //Establishing connection with all the servers in system
                            string[] ipList = ServerCommunication.Connect(ip);
                            File.WriteAllLines(CommonFilePaths.ConfigFile, ipList);
                            foreach (var item in ipList)
                            {
                                if (!item.Equals(State.LocalEndPoint.ToString()) && !item.Equals(ip))
                                    ServerCommunication.Connect(item);
                            }
                            var servers = ServerList.GetServers();

                            //Sending request for filetree on startup
                            if (servers != null)
                            {
                                var r = new Request { Command = Command.requestFileTree };
                                Network.Send(servers[0], r.SerializeToByteArray());
                            }
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{ip} not available");
                            var list = ips.ToList();
                            list.Remove(ip);
                            File.WriteAllLines(CommonFilePaths.ConfigFile, list);
                        }
                    }
                }
            }

            Task requestAcceptor = Task.Run(() => AcceptRequest(listener));
            Task requestProcessor = Task.Run(() => RequestProcessor.DequeueRequestFromQueue());
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
