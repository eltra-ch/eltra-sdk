using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EltraCommon.Helpers
{
    public class AppHelper
    {
        public static string GetProductName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            return fileVersionInfo.ProductName;
        }

        public static string GetProcessDir()
        {
            string result = string.Empty;

            var process = Process.GetCurrentProcess();
            var fullPath = process?.MainModule?.FileName;

            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);

                result = fileInfo.DirectoryName;
            }

            return result;
        }

        public static string GetProcessFileName(bool withExtension = true)
        {
            string result = string.Empty;

            var process = Process.GetCurrentProcess();
            var fullPath = process?.MainModule?.FileName;

            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);

                if (!withExtension)
                {
                    result = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                }
                else
                {
                    result = fileInfo.Name;
                }
            }

            return result;
        }
    }
}
