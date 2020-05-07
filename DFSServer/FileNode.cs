using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer
{
    [Serializable]
    public class FileNode
    {
        public FileNode(string name)
        {
            Name = name;
            IPEndPointString = new List<string>();
        }
        public string Name { get; set; }
        public List<string> IPEndPointString { get; set; }
    }
}
