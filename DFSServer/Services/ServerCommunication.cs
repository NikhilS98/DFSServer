using DFSServer.Connections;
using DFSServer.Helpers;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DFSServer.Communication;

namespace DFSServer.Services
{
    public static class ServerCommunication
    {
        public static string[] Connect(string ip)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ip.Substring(0, ip.IndexOf(":")),
                    Convert.ToInt32(ip.Substring(ip.IndexOf(":") + 1)));

                //server will check whether the request is secretKey
                Request request = new Request
                {
                    Command = Command.serverConnect,
                    Parameters = new object[] { Secret.SecretKey,
                        State.LocalEndPoint.Port + ""}
                };

                Network.Send(server, request.SerializeToByteArray());
                var buff = Network.Receive(server, 100000);

                var ipList = buff.Deserialize<string[]>();

                ServerList.Add(server, ip);
                Task.Run(() => ListenServer(server));

                return ipList;

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());

            }

            return null;
        }

        public static void AcceptConnection(Request request, Socket socket)
        {
            object[] parameters = request.Parameters;
            string secret = (string)parameters[0];
            if (secret.Equals(Secret.SecretKey))
            {
                //make a list ips of connected server from serveList
                int port = Convert.ToInt32((string)parameters[1]);
                string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();

                File.AppendAllText(CommonFilePaths.ConfigFile, ip + ":" + port);
                var ips = File.ReadAllLines(CommonFilePaths.ConfigFile);

                ServerList.Add(socket, ip + ":" + port);

                var buffer = ips.SerializeToByteArray();
                Network.Send(socket, buffer);

                Task.Run(() => ListenServer(socket));
            }
            else
            {
                Console.WriteLine($"{socket.RemoteEndPoint.ToString()} secret key did not match");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        public static void Broadcast(byte[] buffer)
        {
            foreach (var server in ServerList.GetServers())
            {
                Network.Send(server, buffer);
            }
        }

        private static void ListenServer(Socket server)
        {
            while (true)
            {
                byte[] buff = null;
                try
                {
                    buff = Network.Receive(server, 100000);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"server {server.RemoteEndPoint.ToString()} disconnected: {e.Message}");
                    ServerList.Remove(server);
                    break;
                }

                //What to do with this lets see
                Response response = null;
                Request request = null;
                try
                {
                    request = buff.Deserialize<Request>();
                    MessageQueue.Enqueue(new MessageQueueItem { Client = server, Request = request });
                }
                catch (Exception e)
                {
                    response = buff.Deserialize<Response>();
                    ResponseParser.Parse(response);
                }
            }
        }
    }
}
