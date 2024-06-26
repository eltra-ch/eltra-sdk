using EltraCommon.Logger;
using EltraUiCommon.Framework;
using EltraUiCommon.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EltraUiCommon.Controls
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Private fields

        private BaseViewModel _parent;
        private string _title = string.Empty;
        private bool _isBusy;
        private bool _isVisible;
        private IInvokeOnMainThread _invokeOnMainThread;

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

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        protected IInvokeOnMainThread InvokeOnMainThread => _invokeOnMainThread;

        #endregion

        #region Methods

        public void Init(IInvokeOnMainThread invokeOnMainThread)
        {
            _invokeOnMainThread = invokeOnMainThread;
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "", Action onChanged = null)
        {
            bool result = false;

            try
            {
                if (!EqualityComparer<T>.Default.Equals(backingStore, value))
                {
                    backingStore = value;

                    ThreadHelper.RunOnMainThread(() =>
                    {
                        onChanged?.Invoke();

                        OnPropertyChanged(propertyName);
                    });

                    result = true;
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SetProperty",e);
            }

            return result;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;

            try
            {
                changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch(Exception)
            {
            }
        }

        #endregion
    }
}
