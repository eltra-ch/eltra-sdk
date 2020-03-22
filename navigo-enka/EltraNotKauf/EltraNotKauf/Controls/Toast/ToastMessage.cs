using Xamarin.Forms;

namespace EltraNotKauf.Controls.Toast
{
    public static class ToastMessage
    {
        public static void ShortAlert(string message)
        {
            var toastMessage = DependencyService.Get<IToastMessage>();

            toastMessage?.ShortAlert(message);
        }

        public static void LongAlert(string message)
        {
            var toastMessage = DependencyService.Get<IToastMessage>();

            toastMessage?.LongAlert(message);
        }
    }
}
