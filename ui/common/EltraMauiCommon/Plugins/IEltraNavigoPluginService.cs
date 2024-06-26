using EltraUiCommon.Controls;
using EltraMauiCommon.Plugins.Events;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using EltraMauiCommon.Dialogs;

namespace EltraMauiCommon.Plugins
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

        List<MauiDialogViewModel> GetDialogViewModels();
        View ResolveDialogView(MauiDialogViewModel viewModel);
        void ShowDialog(object sender, DialogRequestedEventArgs e);

        #endregion
    }
}
