using Android.App;
using Android.Widget;
using EltraNavigo.Controls.Toast;

[assembly: Xamarin.Forms.Dependency(typeof(EltraNavigo.Droid.Controls.Toast.ToastMessage))]
namespace EltraNavigo.Droid.Controls.Toast
{
    public class ToastMessage : IToastMessage
    {
        public void LongAlert(string message)
        {
            Android.Widget.Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Android.Widget.Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}