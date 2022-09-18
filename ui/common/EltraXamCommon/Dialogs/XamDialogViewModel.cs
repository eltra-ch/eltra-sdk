using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace EltraXamCommon.Dialogs
{
    public class XamDialogViewModel : BindableBase, IDialogAware
    {
        #region Events

        public event Action<IDialogParameters> RequestClose;

        #endregion

        #region Events handling

        protected void SendCloseRequest(IDialogParameters dialogParameters = null)
        {
            RequestClose?.Invoke(dialogParameters);
        }

        #endregion

        #region Methods

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {            
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {            
        }

        #endregion
    }
}
