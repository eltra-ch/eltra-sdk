using Xamarin.Forms;

namespace EltraNotKauf.Views.Login.Behaviors
{
    public class LoginValidationBehavior : Behavior<Entry> 
    {
        private Entry _entry;
        private SignViewModel _loginViewModel;
        
        protected override void OnAttachedTo(Entry entry) 
        {
            _entry = entry;
        
            entry.TextChanged += OnEntryTextChanged;
            entry.BindingContextChanged += OnBindingContextChanged;
            
            base.OnAttachedTo(entry);
        }

        private void OnBindingContextChanged(object sender, System.EventArgs e)
        {
            if (((Entry)sender).BindingContext is SignViewModel loginViewModel)
            {
                _loginViewModel = loginViewModel;
                _loginViewModel.StatusChanged += OnLoginViewModelStatusChanged;
            }
        }

        private void OnLoginViewModelStatusChanged(object sender, Events.SignStatusEventArgs e)
        {
            if (e.Status == SignStatus.Failed)
            {
                Shake();
            }
        }

        protected override void OnDetachingFrom(Entry entry) 
        {
            if(_loginViewModel != null)
            {
                _loginViewModel.StatusChanged -= OnLoginViewModelStatusChanged;
            }

            entry.TextChanged -= OnEntryTextChanged;
            
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args) 
        {
            if (((Entry)sender).BindingContext is SignViewModel loginViewModel)
            {
                if (_loginViewModel != loginViewModel)
                {
                    _loginViewModel = loginViewModel;
                }

                loginViewModel.OnLoginChanged(args.NewTextValue);
            }
        }

        private async void Shake()
        {
            uint timeout = 50;

            if (_entry != null)
            {
                _entry.TextColor = Color.Red;

                await _entry.TranslateTo(-15, 0, timeout);

                await _entry.TranslateTo(15, 0, timeout);

                await _entry.TranslateTo(-10, 0, timeout);

                await _entry.TranslateTo(10, 0, timeout);

                await _entry.TranslateTo(-5, 0, timeout);

                await _entry.TranslateTo(5, 0, timeout);

                _entry.TranslationX = 0;
            }
        }

    }
}