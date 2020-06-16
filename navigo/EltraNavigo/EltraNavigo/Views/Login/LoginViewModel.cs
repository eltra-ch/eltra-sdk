using System;
using System.Threading.Tasks;
using System.Windows.Input;
using EltraNavigo.Controls;
using Xamarin.Forms;
using System.ComponentModel;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;

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
            Title = "Eltra Cloud, Please Sign-in";
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

        private string AutoLogonActiveName => $"{GetType().Name}_auto_logon_active";

        #endregion

        #region Command

        public ICommand CancelCommand => new Command(OnCanceled);

        public ICommand LoginCommand => new Command(OnLoginClicked);

        public ICommand TouchLoginCommand => new Command(OnTouchLoginClicked);

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

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

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName== "AutoLogOnActive")
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

        private void OnLoginClicked()
        {
            StoreLoginSettings();

            OnChanged();
        }

        private async void OnTouchLoginClicked()
        {
            var request = new AuthenticationRequestConfiguration("Prove you have fingers!", "Because without it you can't have access");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                // do secret stuff :)
            }
            else
            {
                // not allowed to do secret stuff :(
            }
        }

        private void OnRegisterClicked()
        {
            StoreLoginSettings();

            OnChanged();
        }

        private void UpdateValidFlag()
        {
            IsValid = !string.IsNullOrEmpty(LoginName) && !string.IsNullOrEmpty(Password);
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
        }

        private void StoreAutoLoginSettings()
        {
            Application.Current.Properties[AutoLogonActiveName] = AutoLogOnActive.ToString();
        }

        #endregion        
    }
}
