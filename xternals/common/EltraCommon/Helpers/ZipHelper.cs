using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EltraCommon.Helpers
{
    public static class ZipHelper
    {
        public static byte[] Compress(string text)
        {
            byte[] result;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var file = archive.CreateEntry("file.txt");

                    using (var entryStream = file.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(text);
                        }
                    }
                }

                result = memoryStream.ToArray();
            }

            return result;
        }
             
        public static string Deflate(byte[] compressedArray)
        {
            string result = string.Empty;

            using (var stream = new MemoryStream(compressedArray))
            {
                using (var archive = new ZipArchive(stream))
                {
                    var entry = archive.Entries.FirstOrDefault();

                    if (entry != null)
                    {
                        using (var entryStream = entry.Open())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                entryStream.CopyTo(memoryStream);

                                var plainText = memoryStream.ToArray();

                                result = Encoding.Default.GetString(plainText);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
