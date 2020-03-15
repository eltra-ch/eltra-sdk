using EltraCommon.Helpers;
using EltraNavigo.Controls;
using Xamarin.Forms;

namespace EltraNavigo.Views.About
{
    public class AboutViewModel : ToolViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            Image = ImageSource.FromResource("EltraNavigo.Resources.lightbulb_32px.png");
            IsMandatory = true;
            PutImage = ImageSource.FromResource("EltraNavigo.Resources.put-logo.png");
            Uuid = "0069D91E-6EB9-4C59-9BB8-65996A318740";
        }
        
        public ImageSource PutImage { get; set; }

        public string VersionName => GetOsAndAppVersion();
        
        private string GetOsAndAppVersion()
        {
            string result = string.Empty;
            IVersionHelper helper = DependencyService.Get<IVersionHelper>();

            if (helper != null)
            {
                result = helper.GetAppVersion();
            }

            return result;
        }
    }
}