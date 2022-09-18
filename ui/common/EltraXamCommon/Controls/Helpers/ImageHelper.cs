using EltraCommon.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Helpers
{
    static class ImageHelper
    {
        public static async Task<string> GetImageAsBase64(ImageSource image)
        {
            string result = string.Empty;
            byte[] imageArray = null;

            if (image != null)
            {
                var streamImageSource = (StreamImageSource)image;
                var stream = await streamImageSource.Stream(CancellationToken.None);

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    imageArray = ms.ToArray();
                }

                result = Convert.ToBase64String(imageArray);
            }

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
