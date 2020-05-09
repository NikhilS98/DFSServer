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
            if(response.Request != null && response.Request.Guid != null)
            {
                ServerResponseList.Add(response.Request.Guid, response);
            }
            else if(response.Command == Command.updateFileTree)
            {
                var rootDirNode = response.Bytes.Deserialize<DirectoryNode>();
                FileTree.SetRootDirectory(rootDirNode);
            }

        }
    }
}
