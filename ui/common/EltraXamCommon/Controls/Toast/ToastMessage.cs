using EltraUiCommon.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Toast
{
    public static class ToastMessage
    {
        private static bool _isAlerted;
        private static Stack<string> _pendingMessages = new Stack<string>();

        public static void ShortAlert(string message)
        {
            if (!_isAlerted)
            {
                var toastMessage = DependencyService.Get<IToastMessage>();

                ThreadHelper.RunOnMainThread(
                    () =>
                    {
                        _isAlerted = true;

                        toastMessage?.ShortAlert(message);

                        _isAlerted = false;
                    });
            }
            else
            {
                _pendingMessages.Push(message);

                Task.Run(async () => {

                    await Task.Delay(500);

                    string msg = _pendingMessages.Pop();

                    ShortAlert(msg);
                });
            }
        }

        public static void LongAlert(string message)
        {
            if (!_isAlerted)
            {
                var toastMessage = DependencyService.Get<IToastMessage>();

                ThreadHelper.RunOnMainThread(
                    () =>
                    {
                        _isAlerted = true;

                        toastMessage?.LongAlert(message);

                        _isAlerted = false;
                    });
            }
            else
            {
                _pendingMessages.Push(message);

                Task.Run(async () => {

                    await Task.Delay(500);

                    string msg = _pendingMessages.Pop();

                    ShortAlert(msg);
                });
            }
        }
    }
}
