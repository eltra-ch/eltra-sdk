using Prism.Mvvm;
using Prism.Dialogs;
using System;

namespace EltraXamCommon.Dialogs
{
    public class XamDialogViewModel : BindableBase, IDialogAware
    {

        #region Events

        private DialogCloseListener _dialogCloseListener = new DialogCloseListener();

        DialogCloseListener IDialogAware.RequestClose => _dialogCloseListener;
        
        #endregion

        #region Events handling

        protected void SendCloseRequest(IDialogParameters dialogParameters = null)
        {
            _dialogCloseListener.Invoke(dialogParameters);
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
