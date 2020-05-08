using DFSServer.Helpers;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFSServer.Services
{
    public static class DirectoryService
    {
        //working for relative and absolute
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

        //working for relative and absolute
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

        //working for relative and absolute
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

                //if local delete files from disk
                RemoveFilesByDirectory(directory);
                response.IsSuccess = true;
                response.Message = "Directory removed successfully";

                //else forward
            }

            return response;
        }

        //working for relative and absolute
        public static Response MoveDirectory(string curPath, string newPath)
        {
            var response = ResponseFormation.GetResponse();

            var directory = FileTree.GetDirectory(curPath);
            if (directory == null)
                response.Message = $"Directory {curPath} does not exist";
            else
            {
                DirectoryNode newParent = FileTree.GetDirectory(newPath);
                if (newParent == null)
                    response.Message = $"Directory {newPath} does not exist";
                else {
                    DirectoryNode curParent = FileTree.GetDirectory(PathHelpers.GetDirFromPath(curPath));
                    FileTree.RemoveDirectory(curParent, directory);
                    FileTree.AddDirectory(newParent, directory);

                    response.IsSuccess = true;
                    response.Message = "Directory moved successfully";
                }
            }

            return response;
        }

        //working for relative and absolute
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

        public static void RemoveFilesByDirectory(DirectoryNode directory)
        {
            foreach (var file in directory.Files)
            {
                FileService.RemoveFileFromDisk(file.ImplicitName);
            }
            foreach (var dir in directory.Directories)
            {
                RemoveFilesByDirectory(dir);
            }
        }
    }
}
