using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer
{
    [Serializable]
    public class FileNode
    {
        public FileNode(string name, string implicitName, long size)
        {
            Name = name;
            ImplicitName = implicitName;
            IPAddresses = new List<string>();
            Size = size;
        }
        public string Name { get; set; }
        public string ImplicitName { get; set; }
        public List<string> IPAddresses { get; set; }
        public long Size { get; set; }
    }
}
