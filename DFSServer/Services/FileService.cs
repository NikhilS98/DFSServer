using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFSServer.Services
{
    public class FileService
    {
        public static byte[] OpenFile(string path)
        {
            string text = File.ReadAllText(path);
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
