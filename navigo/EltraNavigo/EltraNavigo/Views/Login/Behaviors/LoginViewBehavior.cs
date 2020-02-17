using System;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login.Behaviors
{
    class LoginViewBehavior : Behavior<ContentView>
    {
        private ContentView _page;
        private LoginViewModel _loginViewModel;

        protected override void OnAttachedTo(ContentView page)
        {
            _page = page;

            page.BindingContextChanged += OnPageBindingContextChanged;

            base.OnAttachedTo(page);

            _page.LayoutChanged += (sender, args) => { OnPageAppearing(sender, args); };
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            _loginViewModel?.Show();
        }

        private void OnPageBindingContextChanged(object sender, EventArgs e)
        {
            if (_page.BindingContext is LoginViewModel model)
            {
                _loginViewModel = model;
            }
        }
    }
}
