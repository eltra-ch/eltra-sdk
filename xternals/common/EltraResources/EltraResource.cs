using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EltraResources
{
    public class EltraResource
    {
        public Assembly Assembly
        {
            get
            {
                return Assembly.GetExecutingAssembly();
            }
        }

        public async Task<string> GetFileContent(string path, string fileName)
        {
            string result = string.Empty;

            var assembly = Assembly.GetExecutingAssembly();

            if (assembly != null)
            {
                var assemblyName = assembly.GetName();
                var resourceStream =
                    assembly.GetManifestResourceStream(
                        $"{assemblyName.Name}.{path}.{fileName}");

                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
                    {
                        result = await reader.ReadToEndAsync();
                    }
                }
            }

            return result;
        }

        public string GetImageSourceName(string path, string fileName)
        {
            var type = typeof(EltraResource);
            var result = $"{type?.Namespace}.{path}.{fileName}";

            return result;
        }
    }
}
