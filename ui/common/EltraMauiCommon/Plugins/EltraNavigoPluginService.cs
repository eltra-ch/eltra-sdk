using EltraUiCommon.Controls;
using EltraMauiCommon.Plugins.Events;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using EltraMauiCommon.Dialogs;

namespace EltraMauiCommon.Plugins
{
    public class EltraNavigoPluginService : IEltraNavigoPluginService
    {
        #region Events

        public event EventHandler<DialogRequestedEventArgs> DialogRequested;

        public virtual List<MauiDialogViewModel> GetDialogViewModels()
        {
            throw new NotImplementedException();
        }

        public virtual List<ToolViewModel> GetViewModels()
        {
            throw new NotImplementedException();
        }

        public virtual View ResolveDialogView(MauiDialogViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public virtual View ResolveView(ToolViewModel viewModel)
        {
            throw new NotImplementedException();
        }
        
        public void ShowDialog(object sender, DialogRequestedEventArgs e)
        {
            e.Sender = sender;

            OnDialogRequested(sender, e);
        }

        #endregion

        #region Event handling

        protected virtual void OnDialogRequested(object sender, DialogRequestedEventArgs e)
        {
            DialogRequested?.Invoke(this, e);
        }

        #endregion
    }
}
