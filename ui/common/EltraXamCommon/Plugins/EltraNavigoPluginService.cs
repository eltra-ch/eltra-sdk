using EltraUiCommon.Controls;
using EltraXamCommon.Dialogs;
using EltraXamCommon.Plugins.Events;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace EltraXamCommon.Plugins
{
    public class EltraNavigoPluginService : IEltraNavigoPluginService
    {
        #region Events

        public event EventHandler<DialogRequestedEventArgs> DialogRequested;

        public virtual List<XamDialogViewModel> GetDialogViewModels()
        {
            throw new NotImplementedException();
        }

        public virtual List<ToolViewModel> GetViewModels()
        {
            throw new NotImplementedException();
        }

        public virtual View ResolveDialogView(XamDialogViewModel viewModel)
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
