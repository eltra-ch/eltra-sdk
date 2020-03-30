using EltraCommon.Helpers;
using EltraNotKauf.Controls;
using System.Reflection;
using Xamarin.Forms;

namespace EltraNotKauf.Views.About
{
    public class AboutViewModel : ToolViewModel
    {
        public AboutViewModel()
        {
            Title = "Über";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.info-circle.png",Assembly.GetExecutingAssembly());
            IsMandatory = true;
            LogoImage = ImageSource.FromResource("EltraNotKauf.Resources.logo_nothilfe.png", Assembly.GetExecutingAssembly());
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