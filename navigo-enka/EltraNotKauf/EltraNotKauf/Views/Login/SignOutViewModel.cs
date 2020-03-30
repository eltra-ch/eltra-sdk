using EltraConnector.Controllers;
using EltraNotKauf.Controls;
using EltraNotKauf.Views.Login.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Login
{
    public class SignOutViewModel : ToolViewModel
    {
        #region Private fields

        private AuthControllerAdapter _authControllerAdapter;

        #endregion

        #region Constructors

        public SignOutViewModel()
        {
            Title = "Ausloggen";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.user.png", Assembly.GetExecutingAssembly());
            IsMandatory = true;
            IsEnabled = false;
            Uuid = "10ED2154-B0B0-4EBA-B74B-6E05BD830802";
            Persistenced = false;
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

        private AuthControllerAdapter AuthControllerAdapter => _authControllerAdapter ?? (_authControllerAdapter = new AuthControllerAdapter(Url));

        #endregion

        #region Methods

        public override async void Show()
        {
            await Task.Delay(200).ContinueWith(DelayedStatusChange());

            base.Show();
        }

        private Func<Task, Task> DelayedStatusChange()
        {
            return async t =>
            {
                if (await AuthControllerAdapter.SignOut())
                {
                    OnSignStatusChanged(SignStatus.SignedOut);
                }
                else
                {
                    OnSignStatusChanged(SignStatus.Failed);
                }
            };
        }

        #endregion
    }
}