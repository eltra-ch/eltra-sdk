using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class DeviceSignInViewModel : LoginViewModel
    {
        public DeviceSignInViewModel()
        {
            Title = "Device Sign In";
        }

        protected override void ReadLoginSettings()
        {
            base.ReadLoginSettings();

            if (Application.Current.Properties.ContainsKey("device_login"))
            {
                LoginName = Application.Current.Properties["device_login"] as string;
            }

            if (Application.Current.Properties.ContainsKey("device_password"))
            {
                Password = Application.Current.Properties["device_password"] as string;
            }
        }

        protected override void StoreLoginSettings()
        {
            base.StoreLoginSettings();

            Application.Current.Properties["device_login"] = LoginName;
            Application.Current.Properties["device_password"] = Password;
        }
    }
}
