using Android.App;
using Android.OS;
using EltraCommon.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(EltraNavigo.Droid.Helpers.VersionHelper))]
namespace EltraNavigo.Droid.Helpers
{
    public class VersionHelper : IVersionHelper
    {
        public string GetAppVersion()
        {
            string result = string.Empty;
            var appContext = Application.Context.ApplicationContext;
            var packageManager = appContext?.PackageManager;

            if (packageManager != null)
            {
                var packageInfo = packageManager.GetPackageInfo(appContext.PackageName, 0);

                result = packageInfo?.VersionName;
            }

            return result;
        }
        
        public string GetOsVersion()
        {
            return Build.VERSION.Release;
        }
    }
}