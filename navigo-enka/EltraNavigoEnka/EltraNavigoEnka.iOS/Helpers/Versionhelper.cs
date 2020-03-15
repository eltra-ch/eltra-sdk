using EltraCommon.Helpers;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(EltraNavigo.iOS.Helpers.VersionHelper))]

namespace EltraNavigo.iOS.Helpers
{
    public class VersionHelper : IVersionHelper
    {
        public string GetAppVersion()
        {
            return NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
        }

        public string GetOsVersion()
        {
            return UIDevice.CurrentDevice.SystemVersion;
        }
    }
}