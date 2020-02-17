
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraCommon.Logger;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Status;
using EltraNavigo.Views.Ppm.Control;
using EltraNavigo.Views.Ppm.Inputs;
using EltraNavigo.Views.Ppm.OpMode;
using EltraNavigo.Views.Ppm.Outputs;
using EltraNavigo.Views.Ppm.Status;
using Xamarin.Forms;

namespace EltraNavigo.Views.Ppm
{
    public class PpmViewModel : ToolViewModel
    {
        #region Private fields
        
        private PpmOutputsViewModel _ppmOutputsViewModel;
        private PpmInputsViewModel _ppmInputsViewModel;
        private PpmControlViewModel _ppmControlViewModel;
        private PpmOpModeViewModel _ppmOpModeViewModel;
        private PpmStatusViewModel _ppmStatusViewModel;
        private ToolStatusViewModel _toolStatusViewModel; 

        #endregion

        #region Constructors

        public PpmViewModel()
        {
            Title = "Profile Position Mode";
            Image = ImageSource.FromResource("EltraNavigo.Resources.map_32px.png");
            Uuid = "18AC4A72-831F-4EB4-9400-C50569D3919A";
    }

        #endregion

        #region Properties

        public PpmOutputsViewModel OutputsViewModel => _ppmOutputsViewModel ?? (_ppmOutputsViewModel = new PpmOutputsViewModel(this));

        public PpmInputsViewModel InputsViewModel => _ppmInputsViewModel ?? (_ppmInputsViewModel = new PpmInputsViewModel(this));

        public PpmControlViewModel ControlViewModel => _ppmControlViewModel ?? (_ppmControlViewModel = new PpmControlViewModel(this) { InputsViewModel = InputsViewModel });

        public PpmOpModeViewModel OpModeViewModel => _ppmOpModeViewModel ?? (_ppmOpModeViewModel = new PpmOpModeViewModel(this, Epos4OperationModes.ProfilePosition));

        public ToolStatusViewModel ToolStatusViewModel => _toolStatusViewModel ?? (_toolStatusViewModel = new ToolStatusViewModel(this));
        
        public PpmStatusViewModel StatusViewModel => _ppmStatusViewModel ?? (_ppmStatusViewModel = new PpmStatusViewModel(this));

        #endregion;

        #region Methods

        protected override Task Update()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Update", "Update PpmViewModel");

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
