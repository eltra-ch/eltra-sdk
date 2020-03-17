using EltraNavigo.Views.About;
using EltraNavigo.Views.Contact;
using EltraNavigo.Views.Login;
using EltraNavigo.Views.Orders;
using Xamarin.Forms;

namespace EltraNavigo.Views.Selectors
{
    public class DetailPageSelector : DataTemplateSelector
    {
        public DataTemplate SignInView { get; set; }

        public DataTemplate SignUpView { get; set; }

        public DataTemplate SignOutView { get; set; }

        public DataTemplate ContactView { get; set; }

        public DataTemplate OrderView { get; set; }

        public DataTemplate AboutView { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate result = SignInView;

            if (item is SignInViewModel)
            {
                result = SignInView;
            }
            else if (item is SignUpViewModel)
            {
                result = SignUpView;
            }
            else if (item is SignOutViewModel)
            {
                result = SignOutView;
            }
            else if (item is ContactViewModel)
            {
                result = ContactView;
            }
            else if (item is OrderViewModel)
            {
                result = OrderView;
            }
            else if (item is AboutViewModel)
            {
                result = AboutView;
            }

            return result;
        }
    }
}
