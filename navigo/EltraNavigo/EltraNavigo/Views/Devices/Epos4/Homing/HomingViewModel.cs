
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraCommon.Logger;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Status;
using EltraNavigo.Views.Homing.Control;
using EltraNavigo.Views.Homing.Inputs;
using EltraNavigo.Views.Homing.OpMode;
using EltraNavigo.Views.Homing.Outputs;
using EltraNavigo.Views.Homing.Status;
using Xamarin.Forms;

namespace EltraNavigo.Views.Homing
{
    public class HomingViewModel : ToolViewModel
    {
        #region Private fields
                
        private HomingOutputsViewModel _outputsViewModel;
        private HomingInputsViewModel _inputsViewModel;
        private HomingControlViewModel _controlViewModel;
        private HomingOpModeViewModel _opModeViewModel;
        private HomingStatusViewModel _statusViewModel;
        private ToolStatusViewModel _toolStatusViewModel; 

        #endregion

        #region Constructors

        public HomingViewModel()
        {
            Title = "Homing Mode";            
            Image = ImageSource.FromResource("EltraNavigo.Resources.anchor_32px.png");
            Uuid = "A6EAAAD9-0B2B-4A2A-A402-E7105A72C5E0";
        }

        #endregion

        #region Properties

        public HomingOutputsViewModel OutputsViewModel => _outputsViewModel ?? (_outputsViewModel = new HomingOutputsViewModel(this));

        public HomingInputsViewModel InputsViewModel => _inputsViewModel ?? (_inputsViewModel = new HomingInputsViewModel(this));

        public HomingControlViewModel ControlViewModel => _controlViewModel ?? (_controlViewModel = new HomingControlViewModel(this) { InputsViewModel = InputsViewModel });

        public HomingOpModeViewModel OpModeViewModel => _opModeViewModel ?? (_opModeViewModel = new HomingOpModeViewModel(this, Epos4OperationModes.Homing));

        public ToolStatusViewModel ToolStatusViewModel => _toolStatusViewModel ?? (_toolStatusViewModel = new ToolStatusViewModel(this));
        
        public HomingStatusViewModel StatusViewModel => _statusViewModel ?? (_statusViewModel = new HomingStatusViewModel(this));

        #endregion;

        #region Methods

        protected override Task Update()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Update", "Update HomingViewModel");

            return Task.Run(UpdateEnabledStatus);
        }

        public override async Task Show()
        {
            IsBusy = true;

            await UpdateEnabledStatus();

            await base.Show();

            IsBusy = false;
        }

        private async Task UpdateEnabledStatus()
        {
            if (Vcs != null)
            {
                bool deviceLocked = await Vcs.IsDeviceLocked(Device);

                ToolStatusViewModel.IsEnabled = deviceLocked;
                InputsViewModel.IsEnabled = deviceLocked;
                ControlViewModel.IsEnabled = deviceLocked;
                OpModeViewModel.IsEnabled = deviceLocked;
                StatusViewModel.IsEnabled = deviceLocked;
            }
        }

        #endregion
    }
}
