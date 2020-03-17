using System.Windows.Input;
using Xamarin.Forms;
using EltraConnector.Controllers;
using EltraCloudContracts.Contracts.Users;
using EltraNavigoEnka.Views.Login;

namespace EltraNavigo.Views.Login
{
    public class SignUpViewModel : SignViewModel
    {
        #region Constructors

        public SignUpViewModel()
        {
            Title = "Sign-up";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.profile-male_32px.png");
            IsMandatory = false;
            Uuid = "D4D4DFEF-3EBE-4C74-8D98-A27D3122AC9F";
        }

        #endregion

        #region Command

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

        #endregion

        #region Methods

        private async void OnRegisterClicked()
        {
            StoreLoginSettings();

            var authData = new UserAuthData() { Login = LoginName, Password = Password };

            if (await AuthControllerAdapter.SignUp(authData))
            {
                if (await AuthControllerAdapter.SignIn(authData))
                {
                    OnSignStatusChanged(SignStatus.SignedIn);
                }
                else
                {
                    OnSignStatusChanged(SignStatus.Failed);
                }               
            }
            else
            {
                OnSignStatusChanged(SignStatus.Failed);
            }            
        }

        #endregion        
    }
}
