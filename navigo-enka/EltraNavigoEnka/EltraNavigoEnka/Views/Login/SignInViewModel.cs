using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class SignInViewModel : SignViewModel
    {
        #region Constructors

        public SignInViewModel()
        {
            Title = "Sign-in";
            Uuid = "1F81E5FD-2F7E-4D06-AB28-BCE50728CC91";
            Image = ImageSource.FromResource("EltraNavigo.Resources.profile-male_32px.png");
        }

        #endregion

        #region Events

        public event EventHandler SignUpRequested;

        #endregion

        #region Command

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

        #endregion

        #region Events handling

        protected virtual void OnSignUpRequested()
        {
            SignUpRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void OnRegisterClicked()
        {
            OnSignUpRequested();
        }

        #endregion        
    }
}
