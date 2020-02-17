using Xamarin.Forms;

namespace EltraNavigo.Views.Login.Behaviors 
{    
    public class LoginValidationBehavior : Behavior<Entry> {

        protected override void OnAttachedTo(Entry entry) 
        {
            entry.TextChanged += OnEntryTextChanged;
            
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry) 
        {
            entry.TextChanged -= OnEntryTextChanged;
            
            base.OnDetachingFrom(entry);
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args) 
        {
            if (((Entry)sender).BindingContext is LoginViewModel loginViewModel)
            {
                loginViewModel.OnLoginChanged(args.NewTextValue);
            }
        }
    }
}