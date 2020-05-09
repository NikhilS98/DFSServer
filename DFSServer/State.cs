using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DFSServer
{
    public static class State
    {
        private static DirectoryInfo rootDirectory;
        public static IPEndPoint LocalEndPoint { get; set; }
        public static Socket Listener { get; set; }

        private static readonly object updateSpaceLock = new object();
        private static long localOccupiedSpace;
        public static long LocalOccupiedSpace
        {
            get
            {
                return localOccupiedSpace;
            }
            set
            {
                lock (updateSpaceLock)
                {
                    localOccupiedSpace = value;
                }
            }
        }
        
        public static DirectoryInfo GetRootDirectory()
        {
            return rootDirectory;
        }

        public static void SetRootDirectory(string path)
        {
            rootDirectory = Directory.CreateDirectory(path);
        }

    }
}
