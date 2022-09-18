using EltraUiCommon.Controls;
using EltraXamCommon.Dialogs;
using EltraXamCommon.Plugins.Events;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace EltraXamCommon.Plugins
{
    [Preserve(AllMembers = true)]
    public interface IEltraNavigoPluginService
    {
        #region Events

        event EventHandler<DialogRequestedEventArgs> DialogRequested;

        #endregion

        #region Views

        List<ToolViewModel> GetViewModels();

        View ResolveView(ToolViewModel viewModel);

        #endregion

        #region Dialog views

        List<XamDialogViewModel> GetDialogViewModels();
        View ResolveDialogView(XamDialogViewModel viewModel);
        void ShowDialog(object sender, DialogRequestedEventArgs e);

        #endregion
    }
}
