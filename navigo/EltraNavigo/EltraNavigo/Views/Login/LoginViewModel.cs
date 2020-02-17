using System;
using System.Threading.Tasks;
using System.Windows.Input;
using EltraNavigo.Controls;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class LoginViewModel : ToolViewModel
    {
        #region Private fields

        private string _loginName;
        private string _password;
        private bool _isValid;
        private bool _autoLogOnActive;

        #endregion

        #region Constructors

        public LoginViewModel()
        {
            Title = "Login";
            Image = ImageSource.FromResource("EltraNavigo.Resources.profile-male_32px.png");
            IsMandatory = true;
            
            ReadAutoLoginSettings();
            ReadLoginSettings();
            UpdateValidFlag();

            PropertyChanged += OnViewModelPropertyChanged;
        }

        #endregion

        #region Events

        public event EventHandler Changed;
        public event EventHandler Canceled;

        #endregion

        #region Properties

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

        public bool AutoLogOnActive
        {
            get => _autoLogOnActive;
            set => SetProperty(ref _autoLogOnActive, value);
        }

        #endregion

        #region Command

        public ICommand CancelCommand => new Command( OnCanceled);

        public ICommand OkCommand => new Command( OkClicked);

        #endregion

        #region Events handling

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCanceled()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName== "AutoLogOnActive")
            {
                Application.Current.Properties["auto_logon_active"] = AutoLogOnActive.ToString();
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

        private void OkClicked()
        {
            Application.Current.Properties["device_login"] = LoginName;
            Application.Current.Properties["device_password"] = Password;
            Application.Current.Properties["auto_logon_active"] = AutoLogOnActive.ToString();

            OnChanged();
        }

        private void UpdateValidFlag()
        {
            IsValid = !string.IsNullOrEmpty(LoginName) && !string.IsNullOrEmpty(Password);
        }

        public override async Task Show()
        {
            IsBusy = true;

            ReadAutoLoginSettings();
            ReadLoginSettings();
            UpdateValidFlag();

            await base.Show();

            IsBusy = false;
        }

        private void ReadLoginSettings()
        {
            if (Application.Current.Properties.ContainsKey("device_login"))
            {
                LoginName = Application.Current.Properties["device_login"] as string;
            }

            if (Application.Current.Properties.ContainsKey("device_password"))
            {
                Password = Application.Current.Properties["device_password"] as string;
            }
        }

        private void ReadAutoLoginSettings()
        {
            if (Application.Current.Properties.ContainsKey("auto_logon_active"))
            {
                if (bool.TryParse(Application.Current.Properties["auto_logon_active"] as string, out bool autoLogOn))
                {
                    AutoLogOnActive = autoLogOn;
                }
            }
        }

        #endregion        
    }
}
