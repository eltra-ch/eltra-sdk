using System.Windows.Input;
using Xamarin.Forms;
using EltraConnector.Controllers;
using EltraCloudContracts.Contracts.Users;
using System;
using EltraNotKauf.Controls.Toast;
using System.Reflection;

namespace EltraNotKauf.Views.Login
{
    public class SignUpViewModel : SignViewModel
    {
        private string _repeatPassword;

        #region Constructors

        public SignUpViewModel()
        {
            Title = "Anmelden";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.user.png", Assembly.GetExecutingAssembly());
            IsMandatory = false;
            Uuid = "D4D4DFEF-3EBE-4C74-8D98-A27D3122AC9F";
        }

        #endregion

        #region Properties

        public string RepeatPassword
        {
            get => _repeatPassword;
            set => SetProperty(ref _repeatPassword, value);
        }

        #endregion

        #region Command

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

        #endregion

        #region Methods

        public void OnRepeatPasswordChanged(string newPassword)
        {
            RepeatPassword = newPassword;

            UpdateValidFlag();
        }

        private async void OnRegisterClicked()
        {
            if (Password != RepeatPassword)
            {
                ToastMessage.ShortAlert("Die angegebenen Passwörter stimmen nicht überein");
            }
            else
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
                        ToastMessage.ShortAlert("Einloggen ist fehlgeschlagen");

                        OnSignStatusChanged(SignStatus.Failed);
                    }
                }
                else
                {
                    ToastMessage.ShortAlert("Anmeldung ist fehlgeschlagen");

                    OnSignStatusChanged(SignStatus.Failed);
                }
            }
        }

        protected override void UpdateValidFlag()
        {
            IsLoginValid = true;

            IsValid = !string.IsNullOrEmpty(LoginName) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(RepeatPassword);
        }

        internal void Reset()
        {
            IsLoginValid = true;
            IsValid = true;
            LoginName = string.Empty;
            Password = string.Empty;
            RepeatPassword = string.Empty;
        }

        #endregion
    }
}
