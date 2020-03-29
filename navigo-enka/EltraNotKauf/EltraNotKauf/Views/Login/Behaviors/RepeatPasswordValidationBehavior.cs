using Xamarin.Forms;

namespace EltraNotKauf.Views.Login.Behaviors
{
    public class RepeatPasswordValidationBehavior : Behavior<Entry> {

        private Entry _entry;
        private SignUpViewModel _signUpViewModel;
        
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
            if (((Entry)sender).BindingContext is SignUpViewModel signUpViewModel)
            {
                if (_signUpViewModel != signUpViewModel)
                {
                    _signUpViewModel = signUpViewModel;
                    _signUpViewModel.StatusChanged += (s, a) => { if(a.Status == SignStatus.Failed) Shake(); };
                }

                signUpViewModel.OnRepeatPasswordChanged(args.NewTextValue);
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