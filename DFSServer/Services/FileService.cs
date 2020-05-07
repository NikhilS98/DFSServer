using DFSServer.Helpers;
using DFSUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace DFSServer.Services
{
    public static class FileService
    {
        public static Response OpenFile(string path)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                var file = FileTree.GetFile(directory, filename);
                if (file == null)
                    return CreateFile(path);
                else if (file.IPEndPointString.Exists(x => x.Equals(State.LocalEndPoint.ToString())))
                {
                    string text = File.ReadAllText(Path.Combine(State.GetRootDirectory().FullName, path));
                    response.IsSuccess = true;
                    response.Data = text;
                }
                else
                {
                    // send request to remote server
                    response.Message = "Forwarded";
                    response.IsSuccess = true;
                }
            }
            return response;
        }

        public static Response CreateFile(string path)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                FileNode file = FileTree.GetFile(directory, filename);
                if (file != null)
                    response.Message = "File already exists";
                else
                {
                    // evaluate where to create it

                    //if create locally
                    File.WriteAllText(path, "");
                    file = new FileNode(filename);
                    file.IPEndPointString.Add(State.LocalEndPoint.ToString());
                    FileTree.AddFile(directory, file);

                    response.IsSuccess = true;
                    response.Message = "Successfully Created";
                }
            }
            return response;
        }

        public static Response RemoveFile(string path)
        {
            var response = ResponseFormation.GetResponse();

            string dirPath = PathHelpers.GetDirFromPath(path);
            string filename = PathHelpers.GetFilenameFromPath(path);

            var directory = FileTree.GetDirectory(dirPath);
            if (directory == null)
                response.Message = "Directory does not exist";
            else
            {
                var file = FileTree.GetFile(directory, filename);
                if (file == null)
                    response.Message = "File does not exist";
                else if (file.IPEndPointString.Equals(State.LocalEndPoint.ToString()))
                {
                    File.Delete(Path.Combine(State.GetRootDirectory().FullName, path));
                    FileTree.RemoveFile(directory, file);
                    response.IsSuccess = true;
                    response.Message = "Succesfully Deleted";
                }
                else
                {
                    // send request to remote server
                    response.IsSuccess = true;
                    response.Message = "Forwarded";
                }
            }
            return response;
        }

        public static string MoveFile(string curPath, string newPath)
        {
            string curDirPath = PathHelpers.GetDirFromPath(curPath);
            string curFilename = PathHelpers.GetFilenameFromPath(curPath);

            string newDirPath = PathHelpers.GetDirFromPath(newPath);
            string newFilename = PathHelpers.GetFilenameFromPath(newPath);

            var curDir = FileTree.GetDirectory(curDirPath);
            if (curDir == null)
                return "Directory 1 does not exist";

            var newDir = FileTree.GetDirectory(newDirPath);
            if (newDir == null)
                return "Directory 2 does not exist";

            var curFile = FileTree.GetFile(curDir, curFilename);
            if (curFile == null)
                return "File 1 does not exist";

            if(FileTree.RemoveFile(curDir, curFile))
            {
                curFile.Name = newFilename;
                FileTree.AddFile(curDir, curFile);
                return "Successfully moved";
            }

            return "File could not be moved";
        }
    }
}
