using EltraCloudContracts.Contracts.Users;
using EltraConnector.Controllers;
using EltraNavigo.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class SignViewModel : ToolViewModel
    {
        #region Private fields

        private string _loginName;
        private string _password;
        private bool _isValid;
        private bool _isLoginValid;
        private bool _autoLogOnActive;

        #endregion

        #region Constructors

        public SignViewModel()
        {
            IsMandatory = true;
            IsLoginValid = true;

            ReadAutoLoginSettings();
            ReadLoginSettings();
            UpdateValidFlag();

            PropertyChanged += OnViewModelPropertyChanged;
        }

        #endregion

        #region Events

        public event EventHandler Changed;
        public event EventHandler Canceled;
        public event EventHandler Failure;

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

        public ICommand CancelCommand => new Command(OnCanceled);

        public ICommand LoginCommand => new Command(OnLoginClicked);

        #endregion

        #region Events handling

        protected virtual void OnChanged()
        {
            IsValid = true;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCanceled()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFailure()
        {
            IsLoginValid = false;

            Failure?.Invoke(this, EventArgs.Empty);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutoLogOnActive")
            {
                StoreAutoLoginSettings();
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

            if (await SignIn())
            {
                OnChanged();
            }
            else
            {
                OnFailure();
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
            var auth = new AuthControllerAdapter(Url);

            if (await auth.SignIn(new UserAuthData() { Login = LoginName, Password = Password }))
            {
                result = true;
            }

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            ReadLoginSettings();
            UpdateValidFlag();

            await base.Show();

            IsBusy = false;
        }

        protected virtual void ReadLoginSettings()
        {
            ReadAutoLoginSettings();
        }

        protected virtual void StoreLoginSettings()
        {
            StoreAutoLoginSettings();
        }

        private void ReadAutoLoginSettings()
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

        private void StoreAutoLoginSettings()
        {
            Application.Current.Properties[AutoLogonActiveName] = AutoLogOnActive.ToString();
            Application.Current.Properties[SignInPropertyUserName] = LoginName;
            Application.Current.Properties[SignInPropertyPassword] = Password;
        }

        #endregion

    }
}
