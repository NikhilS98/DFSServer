using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using DFSServer.Connections;
using System.IO;

namespace DFSServer
{
    public static class FileTree
    {
        private static readonly object updateLock = new object();
        private static DirectoryNode RootDirectory = new DirectoryNode("root");
        public static int Id = 0;

        public static void SetRootDirectory(DirectoryNode directoryNode)
        {
            RootDirectory = directoryNode;
        }

        public static DirectoryNode GetRootDirectory()
        {
            return RootDirectory;
        }

        public static FileNode GetFile(DirectoryNode directory, string filename)
        {
            return directory.Files.FirstOrDefault(x => x.Name.Equals(filename));
        }

        public static bool RemoveFile(DirectoryNode directory, FileNode file)
        {
            return directory.Files.Remove(file);
        }

        public static void AddFile(DirectoryNode directory, FileNode file)
        {
            directory.Files.Add(file);
        }

        public static DirectoryNode GetDirectory(string path)
        {
            string[] tokens = path.Split("\\");
            DirectoryNode directory = RootDirectory;
            if (tokens.Length == 1 && !directory.Name.Equals(tokens[0]))
                return null;
            for (int i = 1; i < tokens.Length; i++)
            {
                directory = directory.Directories.FirstOrDefault(x => x.Name.Equals(tokens[i]));
                if (directory == null)
                    return null;

            }
            return directory;
        }

        public static void AddDirectory(DirectoryNode parentDir, DirectoryNode childDir)
        {
            parentDir.Directories.Add(childDir);
        }

        public static bool RemoveDirectory(DirectoryNode parent, DirectoryNode child)
        {
            return parent.Directories.Remove(child);
        }

        public static string GetNewImplicitName()
        {
            //some checking or something. Perhaps use Guid
            string name = null;
            do {
                name = Guid.NewGuid().ToString();
            }
            while(File.Exists(Path.Combine(State.GetRootDirectory().FullName, name)));
            return name;
        }

        /// <summary>
        /// Returns an empty dictionary if not found. Never null
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, long> GetIPSpacePairs()
        {
            Dictionary<string, long> ipSpaces = new Dictionary<string, long>();

            Queue<DirectoryNode> directoryNodes = new Queue<DirectoryNode>();

            directoryNodes.Enqueue(RootDirectory);
            var ipPorts = ServerList.GetIPPorts();
            if (ipPorts.Count == 0)
                return ipSpaces;

            while(directoryNodes.Count > 0)
            {
                var dirNode = directoryNodes.Dequeue();
                foreach (var file in dirNode.Files)
                {
                    foreach (var ip in file.IPAddresses)
                    {
                        if (ipPorts.Exists(x => x.Equals(ip)))
                        {
                            if (ipSpaces.ContainsKey(ip))
                            {
                                ipSpaces[ip] += file.Size;
                            }
                            else
                                ipSpaces.Add(ip, file.Size);
                        }
                    }
                }
                foreach (var dir in dirNode.Directories)
                {
                    directoryNodes.Enqueue(dir);
                }
            }

            return ipSpaces;
        }
    }
}
