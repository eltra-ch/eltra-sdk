using System.Windows.Input;
using Xamarin.Forms;
using EltraConnector.Controllers;
using EltraCloudContracts.Contracts.Users;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace EltraNavigo.Views.Login
{
    public class SignUpViewModel : SignViewModel
    {
        #region Constructors

        public SignUpViewModel()
        {
            Title = "Sign-up";
            Image = ImageSource.FromResource("EltraNavigo.Resources.profile-male_32px.png");
            IsMandatory = false;
            Uuid = "D4D4DFEF-3EBE-4C74-8D98-A27D3122AC9F";
        }

        #endregion

        #region Properties

        #endregion

        #region Command

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

        #endregion

        #region Methods

        private async void OnRegisterClicked()
        {
            StoreLoginSettings();

            var auth = new AuthControllerAdapter(Url);

            if(await auth.SignUp(new UserAuthData() { Login = LoginName, Password = Password } ))
            {
                OnChanged();
            }
            else
            {
                OnFailure();
            }            
        }

        #endregion        
    }
}
