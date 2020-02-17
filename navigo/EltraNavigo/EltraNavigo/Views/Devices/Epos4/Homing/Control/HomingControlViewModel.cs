using System.ComponentModel;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.UserAgent.Vcs;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;
using EltraNavigo.Device.Epos4.Parameters;
using EltraNavigo.Resources;
using EltraNavigo.Views.Homing.Inputs;

namespace EltraNavigo.Views.Homing.Control
{
    public class HomingControlViewModel : ToolControlViewModel
    {
        #region Private fields

        private readonly StatusWordViewModel _statusWordViewModel;

        private bool _isStartButtonEnabled;
        private bool _isHaltButtonEnabled;
        private bool _isEnableButtonEnabled;
        private bool _isQuickStopButtonEnabled;
        private bool _isDefinePositionButtonEnabled;
        private string _enableButtonText;

        #endregion

        #region Constructors

        public HomingControlViewModel(ToolViewModel parent) : base(parent)
        {
            _statusWordViewModel = new StatusWordViewModel(this);
        }

        #endregion

        #region Properties

        public HomingInputsViewModel InputsViewModel { get; set; }

        public bool IsStartButtonEnabled
        {
            get => _isStartButtonEnabled;
            set => SetProperty(ref _isStartButtonEnabled, value);
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

        public bool IsDefinePositionButtonEnabled
        {
            get => _isDefinePositionButtonEnabled;
            set => SetProperty(ref _isDefinePositionButtonEnabled, value);
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
            IsStartButtonEnabled = _statusWordViewModel.IsOperationEnabled;
            IsDefinePositionButtonEnabled = _statusWordViewModel.IsOperationEnabled;
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

        public async void Start()
        {
            if (Vcs != null)
            {
                int method = InputsViewModel.HomingMethod.IntValue;

                await EposVcs.FindHome(method);

                ToastMessage.ShortAlert("Find home");
            }            
        }

        public async void DefinePosition()
        {
            if (Vcs != null)
            {
                int targetPosition = (int)InputsViewModel.HomePosition.IntValue;

                await EposVcs.DefinePosition(targetPosition);

                ToastMessage.ShortAlert($"Define position, target = {targetPosition}");
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
