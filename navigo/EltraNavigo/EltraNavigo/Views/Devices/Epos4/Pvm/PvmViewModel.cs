
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraCommon.Logger;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Status;
using EltraNavigo.Views.Pvm.Control;
using EltraNavigo.Views.Pvm.Inputs;
using EltraNavigo.Views.Pvm.OpMode;
using EltraNavigo.Views.Pvm.Outputs;
using EltraNavigo.Views.Pvm.Status;
using Xamarin.Forms;

namespace EltraNavigo.Views.Pvm
{
    public class PvmViewModel : ToolViewModel
    {
        #region Private fields
        
        private PvmOutputsViewModel _pvmOutputsViewModel;
        private PvmInputsViewModel _pvmInputsViewModel;
        private PvmControlViewModel _pvmControlViewModel;
        private PvmOpModeViewModel _pvmOpModeViewModel;
        private PvmStatusViewModel _pvmStatusViewModel;
        private ToolStatusViewModel _toolStatusViewModel;

        #endregion

        #region Constructors

        public PvmViewModel()
        {
            Title = "Profile Velocity Mode";
            Image = ImageSource.FromResource("EltraNavigo.Resources.bike_32px.png");
            Uuid = "13AFB2FD-3238-475B-9F80-27C52A638908";
        }

        #endregion

        #region Properties

        public PvmOutputsViewModel OutputsViewModel => _pvmOutputsViewModel ?? (_pvmOutputsViewModel = new PvmOutputsViewModel(this));

        public PvmInputsViewModel InputsViewModel => _pvmInputsViewModel ?? (_pvmInputsViewModel = new PvmInputsViewModel(this));

        public PvmControlViewModel ControlViewModel => _pvmControlViewModel ?? (_pvmControlViewModel = new PvmControlViewModel(this) { InputsViewModel = InputsViewModel });

        public PvmOpModeViewModel OpModeViewModel => _pvmOpModeViewModel ?? (_pvmOpModeViewModel = new PvmOpModeViewModel(this, Epos4OperationModes.ProfileVelocity));
        
        public PvmStatusViewModel StatusViewModel => _pvmStatusViewModel ?? (_pvmStatusViewModel = new PvmStatusViewModel(this));

        public ToolStatusViewModel ToolStatusViewModel => _toolStatusViewModel ?? (_toolStatusViewModel = new ToolStatusViewModel(this));

        #endregion

        #region Methods

        protected override Task Update()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Update", "Update PvmViewModel");

            return Task.Run(UpdateEnabledStatus);
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

        public override async Task Show()
        {
            IsBusy = true;

            await UpdateEnabledStatus();

            await base.Show();

            IsBusy = false;
        }

        #endregion
    }
}
