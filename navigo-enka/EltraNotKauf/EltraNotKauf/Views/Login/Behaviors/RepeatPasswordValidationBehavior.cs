using Xamarin.Forms;

namespace EltraNotKauf.Views.Login.Behaviors
{
    public class RepeatPasswordValidationBehavior : PasswordValidationBehavior
    {
        private SignUpViewModel _signUpViewModel;
        
        protected override void OnEntryTextChanged(object sender, TextChangedEventArgs args) 
        {
            if (((Entry)sender).BindingContext is SignUpViewModel signUpViewModel)
            {
                if (_signUpViewModel != signUpViewModel)
                {
                    _signUpViewModel = signUpViewModel;
                }

                signUpViewModel.OnRepeatPasswordChanged(args.NewTextValue);
            }
        }
    }
}