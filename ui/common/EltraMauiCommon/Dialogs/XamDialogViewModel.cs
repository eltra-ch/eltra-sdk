﻿using EltraUiCommon.Controls;
using EltraUiCommon.Dialogs;
using System;

namespace EltraMauiCommon.Dialogs
{
    public class MauiDialogViewModel : BindableBase, IDialogAware
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
