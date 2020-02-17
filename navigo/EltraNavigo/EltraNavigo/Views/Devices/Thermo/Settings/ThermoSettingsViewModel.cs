using System.Threading.Tasks;
using Xamarin.Forms;
using EltraNavigo.Views.Devices.Thermo.Base;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Devices.Thermo.Settings
{
    public class ThermoSettingsViewModel : ThermoToolViewModel
    {
        #region Private fields

        private ParameterEditViewModel _samplingTime;
        private ParameterEditViewModel _reactionInertia;

        #endregion

        #region Constructors

        public ThermoSettingsViewModel()
        {
            UpdateViewModels = false;

            Title = "Settings";
            Image = ImageSource.FromResource("EltraNavigo.Resources.bike_32px.png");
            Uuid = "14CB39F2-F2BE-46CC-AEB6-72201B2021A0";

            _samplingTime = new ParameterEditViewModel(this, "PARAM_SamplingTime");
            _reactionInertia = new ParameterEditViewModel(this, "PARAM_ReactionInertia");

        }

        #endregion

        #region Commands 

        #endregion

        #region Properties

        public ParameterEditViewModel SamplingTime
        {
            get => _samplingTime;
            set => SetProperty(ref _samplingTime, value);
        }

        public ParameterEditViewModel ReactionInertia
        {
            get => _reactionInertia;
            set => SetProperty(ref _reactionInertia, value);
        }

        #endregion

        #region Events handling

        #endregion

        #region Methods

        public override async Task Show()
        {
            IsBusy = true;

            SamplingTime.InitModelData();
            ReactionInertia.InitModelData();

            IsBusy = false;

            await base.Show();
        }

        #endregion
    }
}
