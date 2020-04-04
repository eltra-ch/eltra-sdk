using EltraNotKauf.Helpers;
using Xamarin.Forms;

namespace EltraNotKauf.Controls.Toast
{
    public static class ToastMessage
    {
        public static void ShortAlert(string message)
        {
            ThreadHelper.RunOnMainThread(() => 
            {
                var toastMessage = DependencyService.Get<IToastMessage>();

                toastMessage?.ShortAlert(message);
            });            
        }

        public static void LongAlert(string message)
        {
            ThreadHelper.RunOnMainThread(() =>
            {
                var toastMessage = DependencyService.Get<IToastMessage>();

                toastMessage?.LongAlert(message);
            });
        }
    }
}
