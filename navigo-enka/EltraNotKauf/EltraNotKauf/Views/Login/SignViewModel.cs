using EltraCloudContracts.Contracts.Users;
using EltraConnector.Controllers;
using EltraNotKauf.Controls;
using EltraNotKauf.Views.Login.Events;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Login
{
    public class SignViewModel : ToolViewModel
    {
        #region Private fields

        private string _loginName;
        private string _password;
        private bool _isValid;
        private bool _isLoginValid;
        private bool _autoLogOnActive;
        private AuthControllerAdapter _authControllerAdapter;

        #endregion

        #region Constructors

        public SignViewModel()
        {
            IsMandatory = true;
            IsLoginValid = true;

            ReadStoredLoginData();

            PropertyChanged += OnViewModelPropertyChanged;
        }

        #endregion

        #region Events

        public event EventHandler<SignStatusEventArgs> StatusChanged;

        #endregion

        #region Event handler

        protected void OnSignStatusChanged(SignStatus status)
        {
            StatusChanged?.Invoke(this, new SignStatusEventArgs() { Status = status });
        }

        #endregion

        #region Properties

        public string Url
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("url"))
                {
                    result = Application.Current.Properties["url"] as string;
                }

                return result;
            }
        }

        protected AuthControllerAdapter AuthControllerAdapter => _authControllerAdapter ?? (_authControllerAdapter = new AuthControllerAdapter(Url));

        public string LoginName
        {
            get => _loginName;
            set => SetProperty(ref _loginName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public bool IsLoginValid
        {
            get => _isLoginValid;
            set => SetProperty(ref _isLoginValid, value);
        }

        public bool AutoLogOnActive
        {
            get => _autoLogOnActive;
            set => SetProperty(ref _autoLogOnActive, value);
        }

        protected string AutoLogonActiveName => $"{GetType().Name}_auto_logon_active";

        protected string SignInPropertyUserName => $"{GetType().Name}_sign_in_user_name";

        protected string SignInPropertyPassword => $"{GetType().Name}_sign_in_password";

        #endregion

        #region Command

        public ICommand LoginCommand => new Command(OnLoginClicked);

        #endregion

        #region Events handling
        
        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutoLogOnActive")
            {
                StoreLoginSettings();
            }
        }

        #endregion

        #region Methods
        
        public void OnPasswordChanged(string newPassword)
        {
            Password = newPassword;

            UpdateValidFlag();
        }

        public void OnLoginChanged(string login)
        {
            LoginName = login;

            UpdateValidFlag();
        }

        private async void OnLoginClicked()
        {
            StoreLoginSettings();

            await SignOut();

            if (await SignIn())
            {
                IsValid = true;

                OnSignStatusChanged(SignStatus.SignedIn);
            }
            else
            {
                IsLoginValid = false;

                OnSignStatusChanged(SignStatus.Failed);
            }
        }

        private void UpdateValidFlag()
        {
            IsLoginValid = true;

            IsValid = !string.IsNullOrEmpty(LoginName) && !string.IsNullOrEmpty(Password);
        }

        public async Task<bool> SignIn()
        {
            bool result = false;

            if (IsConnected)
            {
                if (await AuthControllerAdapter.SignIn(new UserAuthData() { Login = LoginName, Password = Password }))
                {
                    result = true;
                }
            }

            return result;
        }

        public async Task<bool> SignOut()
        {
            bool result = false;

            if (IsConnected)
            {
                if (await AuthControllerAdapter.SignOut())
                {
                    OnSignStatusChanged(SignStatus.SignedOut);

                    result = true;
                }
            }

            return result;
        }

        public override void Show()
        {
            IsBusy = true;

            ReadStoredLoginData();

            UpdateValidFlag();

            base.Show();

            IsBusy = false;
        }

        private void ReadStoredLoginData()
        {
            if (Application.Current.Properties.ContainsKey(AutoLogonActiveName))
            {
                if (bool.TryParse(Application.Current.Properties[AutoLogonActiveName] as string, out bool autoLogOn))
                {
                    AutoLogOnActive = autoLogOn;
                }
            }

            if (Application.Current.Properties.ContainsKey(SignInPropertyUserName))
            {
                if (Application.Current.Properties[SignInPropertyUserName] is string val)
                {
                    LoginName = val;
                }
            }

            if (Application.Current.Properties.ContainsKey(SignInPropertyPassword))
            {
                if (Application.Current.Properties[SignInPropertyPassword] is string val)
                {
                    Password = val;
                }
            }
        }

        protected void StoreLoginSettings()
        {
            Application.Current.Properties[AutoLogonActiveName] = AutoLogOnActive.ToString();
            Application.Current.Properties[SignInPropertyUserName] = LoginName;
            Application.Current.Properties[SignInPropertyPassword] = Password;
        }

        #endregion

    }
}
