using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RBL.GitHub.Scrapper.Domain
{
   
    public static class Utilities
    { 
        //private static readonly string pathAppData = Path.Combine($"{Environment.CurrentDirectory}", "App_Data");
        private static readonly string pathAppData = Path.Combine("", "App_Data");
        public static string SaveFile(string contents,string fileName, string extension)
        {
            fileName = $"{NormalizeFileName(fileName)}.{extension}";

            if(!Directory.Exists(pathAppData))
            {
                Directory.CreateDirectory(pathAppData);
            }

            string destPath = Path.Combine($"{pathAppData}", fileName);
            File.WriteAllText(destPath, contents);

            return fileName;
        }

        public static string NormalizeFileName(string fileName)
        {
            fileName = fileName.ToLower()
                    .Replace("https://", "")
                    .Replace("http://", "")
                    .Replace("github.com/", "")
                    .Replace("/", " ")
                    .Replace(".", " ");
            return fileName;
        }

        public static string GetFileContent(string fileName,string extension)
        {
            fileName = $"{NormalizeFileName(fileName)}.{extension}";
            string path = Path.Combine(pathAppData, fileName);

            if (!File.Exists(path))
                return null;

            string fileContent = File.ReadAllText(path);

            return fileContent;
        }
    }
}
