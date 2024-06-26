using EltraMauiCommon.Dialogs;
using EltraUiCommon.Dialogs;
using System;
using System.ComponentModel;

namespace EltraMauiCommon.Plugins.Events
{
    public class DialogRequestedEventArgs : EventArgs, INotifyPropertyChanged 
    {
        private IDialogResult _dialogResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public object Sender { get; set; }

        public MauiDialogViewModel ViewModel { get; set; }
        
        public IDialogParameters Parameters { get; set; }

        public IDialogResult DialogResult 
        { 
            get => _dialogResult; 
            set
            {
                _dialogResult = value;

                OnPropertyChanged("DialogResult");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
