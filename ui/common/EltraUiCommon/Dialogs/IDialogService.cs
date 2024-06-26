using EltraMauiCommon.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EltraUiCommon.Dialogs
{
    public interface IDialogService
    {
        void ShowDialog(string dialogViewName, IDialogParameters parameters, Action<IDialogResult> dialogResult);
    }
}
