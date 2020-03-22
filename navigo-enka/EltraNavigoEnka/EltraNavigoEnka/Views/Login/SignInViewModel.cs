﻿using EltraNavigoEnka.Views.Login;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login
{
    public class SignInViewModel : SignViewModel
    {
        #region Constructors

        public SignInViewModel()
        {
            Title = "Einloggen";
            Uuid = "1F81E5FD-2F7E-4D06-AB28-BCE50728CC91";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.user.png");
        }

        #endregion

        #region Command

        public ICommand RegisterCommand => new Command(OnRegisterClicked);

        #endregion

        #region Methods

        private void OnRegisterClicked()
        {
            OnSignStatusChanged(SignStatus.SignUpRequested);
        }

        #endregion        
    }
}
