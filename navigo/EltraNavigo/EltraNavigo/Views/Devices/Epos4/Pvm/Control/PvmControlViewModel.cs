using System.ComponentModel;
using System.Threading.Tasks;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;
using EltraNavigo.Device.Epos4.Parameters;
using EltraNavigo.Resources;
using EltraNavigo.Views.Pvm.Inputs;

namespace EltraNavigo.Views.Pvm.Control
{
    public class PvmControlViewModel : ToolControlViewModel
    {
        #region Private fields

        private readonly StatusWordViewModel _statusWordViewModel;

        private bool _isSetVelocityButtonEnabled;
        private bool _isHaltButtonEnabled;
        private bool _isEnableButtonEnabled;
        private bool _isQuickStopButtonEnabled;
        private string _enableButtonText;

        #endregion

        #region Constructors

        public PvmControlViewModel(ToolViewModel parent) : base(parent)
        {
            _statusWordViewModel = new StatusWordViewModel(this);
        }

        #endregion

        #region Properties

        public PvmInputsViewModel InputsViewModel { get; set; }

        public bool IsSetVelocityButtonEnabled
        {
            get => _isSetVelocityButtonEnabled;
            set => SetProperty(ref _isSetVelocityButtonEnabled, value);
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
            if(e.PropertyName == "StatusWord")
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
            IsSetVelocityButtonEnabled = _statusWordViewModel.IsOperationEnabled;
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
                await EposVcs.HaltVelocityMovement();

                ToastMessage.ShortAlert("Halt");
            }
        }

        public async void SetVelocity()
        {
            if (Vcs != null)
            {
                if (await EposVcs.MoveWithVelocity((int) InputsViewModel.TargetVelocity.IntValue))
                {
                    ToastMessage.ShortAlert($"Moving with velocity {InputsViewModel.TargetVelocity.IntValue} {InputsViewModel.TargetVelocity.Unit}");
                }
                else
                {
                    ToastMessage.LongAlert($"Moving with velocity {InputsViewModel.TargetVelocity.IntValue} {InputsViewModel.TargetVelocity.Unit} failed!");
                }
            }
        }

        public async void QuickStop()
        {
            if (EposVcs != null)
            {
                if (_statusWordViewModel.IsOperationEnabled)
                {
                    if(await EposVcs.SetQuickStopState())
                    {
                        ToastMessage.ShortAlert("Quickstop");
                    }
                    else
                    {
                        ToastMessage.ShortAlert("Quickstop failed!");
                    }
                }
                else
                {
                    ToastMessage.ShortAlert("Device is not enabled!");
                }
            }            
        }

        public async void EnableDisable()
        {
            if (Vcs != null)
            {
                if (_statusWordViewModel.IsOperationEnabled)
                {
                    if(await EposVcs.SetDisableState())
                    {
                        ToastMessage.ShortAlert("Disabled");
                    }
                }
                else if (_statusWordViewModel.IsDisabled || _statusWordViewModel.IsQuickStopActive || _statusWordViewModel.IsSwitchOnDisabled)
                {
                    if(await EposVcs.SetEnableState())
                    {
                        ToastMessage.ShortAlert("Enabled");
                    }
                }
            }            
        }
        
        #endregion
    }
}
