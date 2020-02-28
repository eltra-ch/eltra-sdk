using System;
using System.Reflection;

namespace EltraResources.Helpers
{
    public static class ResourceHelper
    {
        public static bool GetBase64ImageFromAppResources(string fileName, out string base64String)
        {
            bool result = false;
            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName();
            var resourceStream = assembly.GetManifestResourceStream($"{assemblyName.Name}.Resources.{fileName}");

            base64String = string.Empty;

            if (resourceStream != null)
            {
                byte[] imageBytes = new byte[resourceStream.Length];

                if (resourceStream.Read(imageBytes, 0, (int)resourceStream.Length) == resourceStream.Length)
                {
                    base64String = $"data:image/png;base64, {Convert.ToBase64String(imageBytes)}";

                    result = true;
                }
            }

            return result;
        }

        public static bool GetBase64ImageFromResources(string path, string fileName, out string base64String)
        {
            bool result = false;
            var assembly = Assembly.GetExecutingAssembly();

            base64String = string.Empty;

            var assemblyName = assembly.GetName();
            var resourceStream = assembly.GetManifestResourceStream($"{assemblyName.Name}.{path}.{fileName}");

            if (resourceStream != null)
            {
                byte[] imageBytes = new byte[resourceStream.Length];

                if (resourceStream.Read(imageBytes, 0, (int)resourceStream.Length) == resourceStream.Length)
                {
                    base64String = $"data:image/png;base64, {Convert.ToBase64String(imageBytes)}";

                    result = true;
                }
            }

            return result;
        }
    }
}
