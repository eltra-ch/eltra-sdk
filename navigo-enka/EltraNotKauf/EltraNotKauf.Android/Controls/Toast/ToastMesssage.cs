using Android.App;
using Android.Widget;
using EltraNotKauf.Controls.Toast;

[assembly: Xamarin.Forms.Dependency(typeof(EltraNotKauf.Droid.Controls.Toast.ToastMessage))]
namespace EltraNotKauf.Droid.Controls.Toast
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