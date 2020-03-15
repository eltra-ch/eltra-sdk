using Xamarin.Forms;

namespace EltraNavigo.Views.Login.Behaviors 
{    
    public class PasswordValidationBehavior : Behavior<Entry> {

        private Entry _entry;
        private SignViewModel _loginViewModel;
        
        protected override void OnAttachedTo(Entry entry) 
        {
            _entry = entry;
        
            entry.TextChanged += OnEntryTextChanged;
            
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry) 
        {
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
                    _loginViewModel.Failure += (s, a) => { Shake(); };
                }

                loginViewModel.OnPasswordChanged(args.NewTextValue);
            }
        }

        private async void Shake()
        {
            uint timeout = 50;

            if (_entry != null)
            {
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