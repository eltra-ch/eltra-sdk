using EltraNavigo.Controls;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigoEnka.Controls.Button
{
    public class ThreeStateButtonViewModel : BaseViewModel
    {
        #region Private fields

        private string _label;
        private ButtonState _buttonState;
        private bool _isEnabled;

        #endregion

        #region Constructors

        public ThreeStateButtonViewModel()
        {
            IsEnabled = true;
        }

        #endregion

        #region Properties

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
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

        #endregion

        #region Events

        public ICommand ButtonPressedCommand => new Command(OnButtonPressedCommand);

        public event EventHandler ButtonStateChanged;

        #endregion

        #region Event handling

        protected void OnButtonStateChanged()
        {
            ButtonStateChanged?.Invoke(this, new EventArgs());
        }

        private void OnButtonPressedCommand(object obj)
        {
            if (ButtonState == ButtonState.Unactive)
            {
                ButtonState = ButtonState.Active;
            }
            else if (ButtonState == ButtonState.Active)
            {
                ButtonState = ButtonState.Unactive;
            }

            OnButtonStateChanged();
        }

        #endregion
    }
}
