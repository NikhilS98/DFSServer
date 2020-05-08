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
        public static List<string> Connect(string ip)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ip.Substring(0, ip.IndexOf(":")),
                    Convert.ToInt32(ip.Substring(ip.IndexOf(":") + 1)));

                var buff = Network.ReceiveResponse(server, 10000);

                //server will check whether the request is secretKey
                Request request = new Request
                {
                    Command = Command.serverConnect,
                    Parameters = new object[] { Secret.SecretKey, State.LocalTotalSpace + "",
                        State.LocalEndPoint.Port + ""}
                };

                Network.SendRequest(server, request.SerializeToByteArray());
                buff = Network.ReceiveResponse(server, 1000000);

                var ipList = buff.Deserialize<List<string>>();

                ServerList.Add(server);
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

        public static List<string> AcceptConnection(Request request, Socket socket)
        {
            object[] parameters = request.Parameters;
            string secret = (string) parameters[0];
            if (secret.Equals(Secret.SecretKey))
            {
                int remoteLocalSpace = Convert.ToInt32((string)parameters[1]);
                State.GlobalTotalSpace += remoteLocalSpace;

                //make a list ips of connected server from serveList
                int port = Convert.ToInt32((string)parameters[2]);
                string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();

                File.AppendAllText(CommonFilePaths.ConfigFile, "\n" + ip + ":" + port);
                var ips = File.ReadAllLines(CommonFilePaths.ConfigFile);

                ClientList.Remove(socket.RemoteEndPoint);
                ServerList.Add(socket);

                Task.Run(() => ListenServer(socket));

                return ips.ToList();
            }

            return null;             
        }

        private static void ListenServer(Socket server)
        {
            while (true)
            {
                var bytes = Network.ReceiveResponse(server, 100000);

                //What to do with this lets see
                var response = bytes.Deserialize<Response>();
                ResponseParser.Parse(response);
            }
        }
    }
}
