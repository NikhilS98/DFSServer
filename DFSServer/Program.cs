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
            //creates the dir if it doesn't exist
            State.SetRootDirectory(rootDir);

            RequestListener listener = new RequestListener();
            listener.Listen(100);

            //if(!File.Exists(CommonFilePaths.ConfigFile))
                //File.WriteAllText(CommonFilePaths.ConfigFile, "192.168.0.105:11000\n");
            //CommonFilePaths.ConfigFile = rootDir + "\\config.txt";

            FileTree.ReadFromFile();

            var ips = ConfigurationHelper.Read(CommonFilePaths.ConfigFile);
            if (ips != null)
            {
                foreach (var ip in ips)
                {
                    if (!string.IsNullOrEmpty(ip) && !ip.Equals(State.LocalEndPoint.ToString()))
                    {
                        try
                        {
                            //Establishing connection with all the servers in system
                            var ipList = ServerCommunication.Connect(ip);
                            ConfigurationHelper.Update(CommonFilePaths.ConfigFile, ipList);
                            Console.WriteLine($"Connected to server {ip}");
                            foreach (var item in ipList)
                            {
                                if (!item.Equals(State.LocalEndPoint.ToString()) && !item.Equals(ip))
                                {
                                    ServerCommunication.Connect(item);
                                    Console.WriteLine($"Connected to server {item}");
                                }
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
                            Console.WriteLine($"Server {ip} not available");
                            var list = ips.ToList();
                            list.Remove(ip);
                            ConfigurationHelper.Update(CommonFilePaths.ConfigFile, list);
                        }
                    }
                }
            }

            Task requestAcceptor = Task.Run(() => AcceptRequest(listener));
            Task requestProcessor = Task.Run(() => RequestProcessor.DequeueRequestFromQueue());
            Task inputListener = Task.Run(() => InputListener());
            Task.WaitAll(new Task[] { requestAcceptor, requestProcessor, inputListener});
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

        static void InputListener()
        {
            while (true) 
            {
                string input = Console.ReadLine();
                if (input.Equals("config"))
                {
                    var ips = ConfigurationHelper.Read(CommonFilePaths.ConfigFile).ToList();
                    foreach (var ip in ips)
                    {
                        Console.WriteLine(ip);
                    }
                }
                else if (input.Equals("exit"))
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}
