using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Helpers
{
    public static class ResponseMessages
    {
        public static string DirectoryNotFound { get { return "Directory {0} not found"; } }
        public static string FileNotFound { get { return "File {0} not found in {1}"; } }
    }
}
