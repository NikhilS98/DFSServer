using DFSUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Communication
{
    public static class ResponseParser
    {
        public static void Parse(Response response)
        {
            if(response.Request != null && response.Request.Command == Command.requestFileTree)
            {
                var rootDirNode = response.Bytes.Deserialize<DirectoryNode>();
                FileTree.SetRootDirectory(rootDirNode);
            }
        }
    }
}
