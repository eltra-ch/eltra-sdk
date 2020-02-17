using System.ComponentModel;
using System.Threading.Tasks;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;
using EltraNavigo.Device.Epos4.Parameters;
using EltraNavigo.Resources;
using EltraNavigo.Views.Ppm.Inputs;

namespace EltraNavigo.Views.Ppm.Control
{
    public class PpmControlViewModel : ToolControlViewModel
    {
        #region Private fields

        private readonly StatusWordViewModel _statusWordViewModel;

        private bool _isSetPositionButtonEnabled;
        private bool _isHaltButtonEnabled;
        private bool _isEnableButtonEnabled;
        private bool _isQuickStopButtonEnabled;
        private string _enableButtonText;

        #endregion

        #region Constructors

        public PpmControlViewModel(ToolViewModel parent) : base(parent)
        {
            _statusWordViewModel = new StatusWordViewModel(this);
        }

        #endregion

        #region Properties
        
        public PpmInputsViewModel InputsViewModel { get; set; }

        public bool IsSetPositionButtonEnabled
        {
            get => _isSetPositionButtonEnabled;
            set => SetProperty(ref _isSetPositionButtonEnabled, value);
        }

        public bool IsHaltButtonEnabled
        {
            get => _isHaltButtonEnabled;
            set => SetProperty(ref _isHaltButtonEnabled, value);
        }

        public bool IsEnableButtonEnabled
        {
            get => _isEnableButtonEnabled;
            set => SetProperty(ref _isEnableButtonEnabled, value);
        }

        public bool IsQuickStopButtonEnabled
        {
            get => _isQuickStopButtonEnabled;
            set => SetProperty(ref _isQuickStopButtonEnabled, value);
        }

        public string EnableButtonText
        {
            get => _enableButtonText;
            set => SetProperty(ref _enableButtonText, value);
        }

        #endregion

        #region Events

        private void OnStatusWordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StatusWord")
            {
                UpdateEnableButtonText();

                if (IsEnabled)
                {
                    EnableControlButtons();
                }
            }
        }

        #endregion

        #region Methods

        public override Task Show()
        {
            var result = base.Show();

            UpdateEnableButtonText();

            if (IsEnabled)
            {
                EnableControlButtons();
            }

            if (_statusWordViewModel != null)
            {
                _statusWordViewModel.PropertyChanged += OnStatusWordPropertyChanged;
            }

            return result;
        }

        public override Task Hide()
        {
            if (_statusWordViewModel != null)
            {
                _statusWordViewModel.PropertyChanged -= OnStatusWordPropertyChanged;
            }

            return base.Hide();
        }

        private void UpdateEnableButtonText()
        {
            EnableButtonText = _statusWordViewModel.IsOperationEnabled
                ? AppResources.Disable
                : AppResources.Enable;
        }

        private void EnableControlButtons()
        {
            IsSetPositionButtonEnabled = _statusWordViewModel.IsOperationEnabled;
            IsHaltButtonEnabled = _statusWordViewModel.IsOperationEnabled;

            IsEnableButtonEnabled = _statusWordViewModel.IsOperationEnabled ||
                                    _statusWordViewModel.IsDisabled || _statusWordViewModel.IsQuickStopActive ||
                                    _statusWordViewModel.IsSwitchOnDisabled;
            IsQuickStopButtonEnabled = _statusWordViewModel.IsOperationEnabled;
        }

        public async void Halt()
        {
            if (Vcs != null)
            {
                await EposVcs.HaltPositionMovement();

                ToastMessage.ShortAlert("Halt");
            }            
        }

        public async void SetPosition()
        {
            if (Vcs != null)
            {
                bool absolute = InputsViewModel.IsAbsoluteTarget;
                bool immediately = InputsViewModel.ChangeImmediately;

                await EposVcs.MoveToPosition((int)InputsViewModel.TargetPosition.IntValue, absolute, immediately);

                ToastMessage.ShortAlert($"Move to position = {(int)InputsViewModel.TargetPosition.IntValue}");
            }            
        }

        public async void SetZeroPosition()
        {         
            if (Vcs != null)
            {
                bool absolute = InputsViewModel.IsAbsoluteTarget;
                bool immediately = InputsViewModel.ChangeImmediately;
                int targetPosition = 0;

                await EposVcs.MoveToPosition(targetPosition, absolute, immediately);

                ToastMessage.ShortAlert($"Move to zero position");
            }            
        }

        public async void QuickStop()
        {
            if (Vcs != null)
            {
                if (_statusWordViewModel.IsOperationEnabled)
                {
                    await EposVcs.SetQuickStopState();

                    ToastMessage.ShortAlert("Quick stop");
                }
            }            
        }
        
        public async void EnableDisable()
        {
            if (Vcs != null)
            {
                if (_statusWordViewModel.IsOperationEnabled)
                {
                    await EposVcs.SetDisableState();

                    ToastMessage.ShortAlert("Disabled");
                }
                else if (_statusWordViewModel.IsDisabled || _statusWordViewModel.IsQuickStopActive || _statusWordViewModel.IsSwitchOnDisabled)
                {
                    await EposVcs.SetEnableState();

                    ToastMessage.ShortAlert("Enabled");
                }
            }
        }
        
        #endregion
    }
}
