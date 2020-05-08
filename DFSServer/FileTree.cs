using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace DFSServer
{
    public static class FileTree
    {
        private static readonly object updateLock = new object();
        private static DirectoryNode RootDirectory = new DirectoryNode("root");
        public static int Id = 0;

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
            //some checking or something
            return (Id++) + "";
        }
    }
}
