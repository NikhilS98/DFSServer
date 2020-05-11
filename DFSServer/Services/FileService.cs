using DFSServer.Helpers;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DFSServer.Connections;
using System.Net;
using System.Net.Sockets;

namespace DFSServer.Communication
{
    public static class FileService
    {

        //working for relative and absolute
        public static Response OpenFile(string path)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = string.Format(ResponseMessages.DirectoryNotFound, dirPath);
            else
            {
                var file = FileTree.GetFile(directory, filename);
                if (file == null)
                    return CreateFile(directory, filename, false);
                else if (file.IPAddresses.Exists(x => x.Equals(State.LocalEndPoint.ToString())) && 
                    File.Exists(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName)))
                {
                    string text = File.ReadAllText(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName));
                    response.IsSuccess = true;
                    response.Data = text;
                }
                else
                {
                    // send request to remote server
                    bool exists = false;
                    foreach (var ipPort in ServerList.GetIPPorts())
                    {
                        exists = file.IPAddresses.Exists(x => x.Equals(ipPort));
                        if (exists)
                        {
                            response.Command = Command.forwarded;
                            response.Message = ipPort;
                            response.IsSuccess = true;
                            break;
                        }
                        
                    }

                    if (!exists)
                    {
                        response.Message = string.Format(ResponseMessages.FileNotFound, filename, dirPath);
                    }
                    
                }
            }
            return response;
        }

        //working for relative and absolute
        private static Response CreateFile(DirectoryNode directory, string filename, 
            bool createLocally)
        {
            var response = ResponseFormation.GetResponse();

            //query filetree to find out which server has least storage occupied
            var ipSpacePairs = FileTree.GetIPSpacePairs();
            string ipPort = null;
            if (ipSpacePairs.Count == 0)
                ipPort = State.LocalEndPoint.ToString();
            else
                ipPort = ipSpacePairs.OrderBy(x => x.Value).First().Key;

            if (createLocally || State.LocalEndPoint.ToString().Equals(ipPort))
            {
                FileNode file = new FileNode(filename, FileTree.GetNewImplicitName(), 0);
                File.WriteAllText(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName), "");
                file.IPAddresses.Add(State.LocalEndPoint.ToString());
                FileTree.AddFile(directory, file);

                FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());

                //FileTree.AddInLocalFiles(file);

                response.Message = "Successfully Created";
            }
            else
            {
                //forward
                response.Command = Command.forwarded;
                response.Message = ipPort;
            }

            response.IsSuccess = true;
            
            return response;
        }

        //working for relative and absolute
        public static Response MoveFile(string curPath, string newPath)
        {
            var response = ResponseFormation.GetResponse();

            string curDirPath = PathHelpers.GetDirFromPath(curPath);
            string curFilename = PathHelpers.GetFilenameFromPath(curPath);

            string newDirPath = PathHelpers.GetDirFromPath(newPath);
            string newFilename = PathHelpers.GetFilenameFromPath(newPath);

            var curDir = FileTree.GetDirectory(curDirPath);
            if (curDir == null)
                 response.Message = string.Format(ResponseMessages.DirectoryNotFound, curDirPath);
            else
            {
                var file = FileTree.GetFile(curDir, curFilename);
                if (file == null)
                    response.Message = string.Format(ResponseMessages.FileNotFound, curFilename, curDirPath);
                else
                {
                    var newDir = FileTree.GetDirectory(newDirPath);
                    if (newDir == null)
                        response.Message = string.Format(ResponseMessages.DirectoryNotFound, newDirPath);
                    else
                    {
                        FileTree.RemoveFile(curDir, file);
                        FileTree.AddFile(newDir, file);
                        file.Name = newFilename;
                        response.IsSuccess = true;
                        response.Message = "Successfully Moved";
                        FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());
                    }
                }
            }

            return response;
        }

        //working for relative and absolute
        public static Response UpdateFile(string path, string data)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = string.Format(ResponseMessages.DirectoryNotFound, dirPath);
            else
            {
                var file = FileTree.GetFile(directory, filename);
                var str = State.LocalEndPoint.ToString();
                if (file == null)
                    response.Message = string.Format(ResponseMessages.FileNotFound, filename, dirPath);
                else if (file.IPAddresses.Exists(x => x.Equals(State.LocalEndPoint.ToString())))
                {
                    File.WriteAllText(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName), data);
                    response.IsSuccess = true;
                    response.Message = "Successfully updated";
                    file.Size = data.Length;

                    var servers = ServerList.GetServers();
                    bool isReplicated = false;
                    if (servers.Count > 0)
                    {
                        Request request = new Request();
                        if (file.IPAddresses.Count < 2)
                        {
                            request.Type = "FileService";
                            request.Method = "Replicate";
                            request.Parameters = new object[] { data, file };
                            int index = new Random().Next(servers.Count);
                            file.IPAddresses.Add(ServerList.GetServerList()
                                .FirstOrDefault(x => x.Socket.Equals(servers[index])).IPPort);
                            ServerCommunication.Send(servers[index], request.SerializeToByteArray());
                            FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());
                            isReplicated = true;
                        }
                        else
                        {
                            request.Command = Command.updateFile;
                            request.Method = "UpdateFile";
                            request.Type = "FileService";
                            request.Parameters = new object[] { path, data };
                            foreach(var ip in file.IPAddresses)
                            {
                                var s = ServerList.GetServerList().FirstOrDefault(x => x.IPPort.Equals(ip));
                                if (s != null)
                                {
                                    ServerCommunication.Send(s.Socket, request.SerializeToByteArray());
                                }
                                
                            }
                        }

                        //response.Command = Command.wait;
                    }

                    if(!isReplicated)
                        FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());

                }
                else
                {
                    string ip = null;
                    foreach (var ipPort in ServerList.GetIPPorts())
                    {
                        if(file.IPAddresses.Exists(x => x.Equals(ipPort)))
                        {
                            ip = ipPort;
                            break;
                        }
                    }
                    if (ip == null)
                    {
                        response.Message = string.Format(ResponseMessages.FileNotFound, filename, dirPath);
                    }
                    else
                    {
                        // send request to remote server
                        response.Message = ip;
                        response.IsSuccess = true;
                        response.Command = Command.forwarded;
                    }
                }
            }
            return response;
        }

        public static Response Replicate(string data, FileNode file)
        {
            var response = ResponseFormation.GetResponse();
            File.WriteAllText(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName), data);

            //FileTree.AddInLocalFiles(file);
            response.IsSuccess = true;
            response.Message = "Successfully replicated";

            Console.WriteLine($"{file.Name} replicated");

            return response;
        }

        //working for relative and absolute
        public static Response RemoveFile(string path)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = string.Format(ResponseMessages.DirectoryNotFound, dirPath);
            else
            {
                var file = FileTree.GetFile(directory, filename);
                if (file == null)
                    response.Message = string.Format(ResponseMessages.FileNotFound, filename, dirPath);
                else if (file.IPAddresses.Exists(x => x.Equals(State.LocalEndPoint.ToString())) &&
                    File.Exists(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName)))
                {
                    RemoveFileFromDisk(file.ImplicitName);
                    FileTree.RemoveFile(directory, file);
                    response.IsSuccess = true;
                    response.Message = "Succesfully Deleted";
                    FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());

                    if(file.IPAddresses.Count > 1)
                    {
                        var remoteIps = file.IPAddresses.Where(x => !x.Equals(State.LocalEndPoint.ToString())).ToList();
                        if(remoteIps != null)
                        {
                            foreach (var ip in remoteIps)
                            {
                                var socket = ServerList.GetServerList()
                                    .FirstOrDefault(x => x.IPPort.Equals(ip)).Socket;
                                var request = new Request
                                {
                                    Type = "FileService",
                                    Method = "RemoveFileFromDisk",
                                    Parameters = new object[] { file.ImplicitName }
                                };
                                ServerCommunication.Send(socket, request.SerializeToByteArray());
                            }
                        }
                    }
                }
                else
                {
                    // send request to remote server
                    response.IsSuccess = true;
                    response.Message = "Forwarded";
                    response.Command = Command.forwarded;
                }
            }
            return response;
        }

        public static Response RemoveFileFromDisk(string implicitName)
        {
            File.Delete(Path.Combine(State.GetRootDirectory().FullName, implicitName));
            return new Response();
        }
    }
}
