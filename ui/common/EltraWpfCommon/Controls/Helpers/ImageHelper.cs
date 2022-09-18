using EltraCommon.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EltraWpfCommon.Controls.Helpers
{
    static class ImageHelper
    {
        private static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        public static async Task<string> GetImageAsBase64(ImageSource image)
        {
            string result = string.Empty;

            await Task.Run(() => {
                if (image != null)
                {
                    var encoder = new PngBitmapEncoder();

                    var imageArray = ImageSourceToBytes(encoder, image);

                    result = Convert.ToBase64String(imageArray);
                }
            });
            
            return result;
        }

        public static async Task<string> GetImageHashCode(ImageSource image)
        {
            var base64 = await GetImageAsBase64(image);

            string result = CryptHelpers.ToMD5(base64);

            return result;
        }
    }
}
