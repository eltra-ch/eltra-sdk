using System.Diagnostics;
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
    }
}
