using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EltraNavigo.Controls
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Private fields

        private BaseViewModel _parent;
        private string _title = string.Empty;
        private bool _isBusy;

        #endregion

        #region Constructors

        public BaseViewModel()
        {
        }

        public BaseViewModel(BaseViewModel parent)
        {
            Parent = parent;
        }

        #endregion

        #region Properties

        public BaseViewModel Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Methods

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "", Action onChanged = null)
        {
            bool result = false;

            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;

                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {

                    onChanged?.Invoke();

                    OnPropertyChanged(propertyName);
                });
                

                result = true;
            }

            return result;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;

            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
