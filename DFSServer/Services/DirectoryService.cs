using DFSServer.Helpers;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer.Services
{
    public static class DirectoryService
    {
        public static Response OpenDirectory(string path)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(path);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                response.IsSuccess = true;
                response.Data = path;
            }
            
            return response;
        }

        public static Response CreateDirectory(string path)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(path);
            if (directory != null)
                response.Message = "Directory already exists";
            else
            {
                directory = new DirectoryNode(PathHelpers.GetFilenameFromPath(path));
                var parentDirStr = PathHelpers.GetDirFromPath(path);
                var parent = FileTree.GetDirectory(parentDirStr);
                if (parent == null)
                {
                    response.Message = $"Directory {parentDirStr} does not exist";
                }
                else
                {
                    FileTree.AddDirectory(parent, directory);
                    response.IsSuccess = true;
                    response.Message = "Directory created successfully";
                }
            }

            return response;
        }

        public static Response RemoveDirectory(string path)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(path);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                DirectoryNode parent = FileTree.GetDirectory(PathHelpers.GetDirFromPath(path));
                FileTree.RemoveDirectory(parent, directory);
                response.IsSuccess = true;
                response.Message = "Directory removed successfully";
            }

            return response;
        }

        /*public static Response MoveDirectory(string curPath, string newPath)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(path);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                DirectoryNode parent = FileTree.GetDirectory(PathHelpers.GetDirFromPath(path));
                FileTree.RemoveDirectory(parent, directory);

                response.IsSuccess = true;
                response.Message = "Directory created successfully";
            }

            return response;
        }*/

        public static Response ListContent(string path)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(path);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                response.IsSuccess = true;
                string msg = "";
                foreach (var dir in directory.Directories)
                {
                    msg += dir.Name + "+\n";
                }
                foreach (var file in directory.Files)
                {
                    msg += file.Name + "\n";
                }

                response.Message = msg;
            }

            return response;
        }
    }
}
