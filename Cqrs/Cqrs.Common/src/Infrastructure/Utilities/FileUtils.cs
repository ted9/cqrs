using System;
using System.IO;
using System.Web;

namespace Cqrs.Infrastructure.Utilities
{
    public static class FileUtils
    {
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static bool FileExists(string fileFullName)
        {
            return File.Exists(fileFullName);
        }

        public static bool FileMove(string sourceFileName, string destFileName)
        {
            if (!FileExists(sourceFileName))
                return false;

            if (string.IsNullOrEmpty(destFileName))
                return false;

            string destFilePath = destFileName.Substring(0, destFileName.LastIndexOf("\\"));

            if (!Directory.Exists(destFilePath)) {
                Directory.CreateDirectory(destFilePath);
            }
            File.Delete(destFileName);
            File.Move(sourceFileName, destFileName);

            return true;
        }

        public static bool FileCopy(string sourceFileName, string destFileName)
        {
            if (!FileExists(sourceFileName))
                return false;

            if (destFileName == string.Empty)
                return false;

            string destFilePath = destFileName.Substring(0, destFileName.LastIndexOf("\\"));

            if (!Directory.Exists(destFilePath)) {
                Directory.CreateDirectory(destFilePath);
            }
            File.Copy(sourceFileName, destFileName, true);

            return true;
        }

        public static void FileDelete(string fileName)
        {
            if (!FileExists(fileName))
                return;

            File.Delete(fileName);
        }

        public static long FileSize(string fileName)
        {
            if (File.Exists(fileName)) {
                return new FileInfo(fileName).Length;
            }

            return -1;
        }

        public static string GetMapPath(string strPath)
        {
            if (HttpContext.Current != null) {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else 
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
        }
    }
}
