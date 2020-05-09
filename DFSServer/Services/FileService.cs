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

namespace DFSServer.Services
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
                    return CreateFile(directory, filename);
                else if (file.IPAddresses.Exists(x => x.Equals(State.LocalEndPoint.ToString())))
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
        private static Response CreateFile(DirectoryNode directory, string filename)
        {
            var response = ResponseFormation.GetResponse();

            //query filetree to find out which server has least storage occupied
            var ipSpacePairs = FileTree.GetIPSpacePairs();
            string ipPort = null;
            if (ipSpacePairs.Count == 0)
                ipPort = State.LocalEndPoint.ToString();
            else
                ipPort = ipSpacePairs.OrderBy(x => x.Value).First().Key;

            if (State.LocalEndPoint.ToString().Equals(ipPort))
            {
                FileNode file = new FileNode(filename, FileTree.GetNewImplicitName(), 0);
                File.WriteAllText(Path.Combine(State.GetRootDirectory().FullName, file.ImplicitName), "");
                file.IPAddresses.Add(State.LocalEndPoint.ToString());
                FileTree.AddFile(directory, file);

                FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());

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
                else if (file.IPAddresses.Exists(x => x.Equals(State.LocalEndPoint.ToString())))
                {
                    RemoveFileFromDisk(file.ImplicitName);
                    FileTree.RemoveFile(directory, file);
                    response.IsSuccess = true;
                    response.Message = "Succesfully Deleted";
                    FileTreeService.UpdateFileTree(FileTree.GetRootDirectory().SerializeToByteArray());
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

        public static void RemoveFileFromDisk(string implicitName)
        {
            File.Delete(Path.Combine(State.GetRootDirectory().FullName, implicitName));
        }
    }
}
