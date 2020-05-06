using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFSServer.Services
{
    public static class FileService
    {
        public static string OpenFile(string path)
        {
            string text = File.ReadAllText(path);
            return text;
        }

    }
}
