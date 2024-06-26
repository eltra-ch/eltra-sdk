using EltraMauiCommon.Dialogs;
using EltraUiCommon.Dialogs;
using System;
using System.Threading.Tasks;

namespace EltraMauiCommon.Controls.Extensions
{
    internal static class DialogExtensions
    {
        public static Task<IDialogResult> ShowDialogAsync2(this IDialogService dialogService, string name, IDialogParameters parameters)
        {
            var tcs = new TaskCompletionSource<IDialogResult>();

            try
            {
                dialogService.ShowDialog(name, parameters, (result) => {
                    if (result.Exception != null)
                    {
                        tcs.SetException(result.Exception);
                        return;
                    }
                    tcs.SetResult(result);
                });
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}
