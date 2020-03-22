using EltraCommon.Helpers;
using EltraNavigo.Controls;
using Xamarin.Forms;

namespace EltraNavigo.Views.About
{
    public class AboutViewModel : ToolViewModel
    {
        public AboutViewModel()
        {
            Title = "Über";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.info-circle.png");
            IsMandatory = true;
            LogoImage = ImageSource.FromResource("EltraNavigoEnka.Resources.logo_nothilfe.png");
            Uuid = "0069D91E-6EB9-4C59-9BB8-65996A318740";
        }
        
        public ImageSource LogoImage { get; set; }

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