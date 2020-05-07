using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DFSServer
{
    public static class State
    {
        private static DirectoryInfo rootDirectory;
        public static IPEndPoint LocalEndPoint { get; set; }
        public static int LocalTotalSpace { get; set; }

        private static int localOccupiedSpace;
        private static readonly object updateSpaceLock = new object();
        
        public static DirectoryInfo GetRootDirectory()
        {
            return rootDirectory;
        }

        public static void SetRootDirectory(string path)
        {
            rootDirectory = Directory.CreateDirectory(path);
        }

        public static bool UpdateOccupiedSpace(int space)
        {
            if (space > LocalTotalSpace || space < 0)
                return false;
            lock (updateSpaceLock)
            {
                localOccupiedSpace = space;
            }
            return true;
        }

        public static int GetOccupiedSpace()
        {
            return localOccupiedSpace;
        }
    }
}
