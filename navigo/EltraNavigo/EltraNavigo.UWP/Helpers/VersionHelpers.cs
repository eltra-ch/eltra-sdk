using Windows.ApplicationModel;
using EltraCommon.Helpers;

namespace EltraNavigo.UWP.Helpers
{
    public class VersionHelper : IVersionHelper
    {
        public string GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        public string GetOsVersion()
        {
            return string.Empty;
        }
    }
}
