using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer
{
    [Serializable]
    public class DirectoryNode
    {
        public DirectoryNode(string name)
        {
            Name = name;
            Directories = new List<DirectoryNode>();
            Files = new List<FileNode>();
        }

        public string Name { get; set; }
        public List<DirectoryNode> Directories { get; set; }
        public List<FileNode> Files { get; set; }
    }
}
