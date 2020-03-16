using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class UserSignInViewModel : LoginViewModel
    {
        public UserSignInViewModel()
        {
            Title = "Sign In";
        }

        protected override void ReadLoginSettings()
        {
            base.ReadLoginSettings();

            if (Application.Current.Properties.ContainsKey("user_login"))
            {
                LoginName = Application.Current.Properties["user_login"] as string;
            }

            if (Application.Current.Properties.ContainsKey("user_password"))
            {
                Password = Application.Current.Properties["user_password"] as string;
            }
        }

        protected override void StoreLoginSettings()
        {
            base.StoreLoginSettings();

            Application.Current.Properties["user_login"] = LoginName;
            Application.Current.Properties["user_password"] = Password;
        }
    }
}
