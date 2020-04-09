using EltraNotKauf.Controls;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNotKauf.Controls.Button
{
    public class ThreeStateButtonViewModel : BaseViewModel
    {
        #region Private fields

        private string _id;
        private ButtonState _buttonState;
        private bool _isEnabled;
        private int _height;
        private bool _longPressActive;

        #endregion

        #region Constructors

        public ThreeStateButtonViewModel()
        {
            IsEnabled = true;
            Height = 84;
        }

        #endregion

        #region Properties

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ButtonState ButtonState
        {
            get => _buttonState;
            set => SetProperty(ref _buttonState, value);
        }

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public bool LongPressActive
        {
            get => _longPressActive;
            set => SetProperty(ref _longPressActive, value);
        }

        #endregion

        #region Events

        public ICommand ButtonPressedCommand => new Command(OnButtonPressedCommand);
        public ICommand ButtonReleasedCommand => new Command(OnButtonReleasedCommand);

        public event EventHandler ButtonStateChanged;

        #endregion

        #region Event handling

        protected void OnButtonStateChanged()
        {
            ButtonStateChanged?.Invoke(this, new EventArgs());
        }

        private void OnButtonPressedCommand()
        {
            if (ButtonState == ButtonState.Inactive)
            {
                ButtonState = ButtonState.Active;
            }
            else if (ButtonState == ButtonState.Active)
            {
                ButtonState = ButtonState.Inactive;
            }

            OnButtonStateChanged();
        }

        private void OnButtonReleasedCommand()
        {
            //TODO
        }

        #endregion
    }
}
