using Xamarin.Forms;
using EltraNavigo.Views.Devices.Thermo.Base;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;

namespace EltraNavigo.Views.Devices.Thermo.Overview
{
    public class ThermoOverviewViewModel : ThermoToolViewModel
    {
        #region Private fields

        private SensorDataViewModel _sensorDataViewModel;
        private ushort _relay1State;
        private Parameter _relayState1Parameter;

        #endregion

        #region Constructors

        public ThermoOverviewViewModel()
        {
            UpdateViewModels = false;

            Title = "Overview";
            Image = ImageSource.FromResource("EltraNavigo.Resources.book-open_32px.png");
            Uuid = "D0CD9514-0796-4795-A730-6D936155DE76";            
        }

        #endregion

        #region Properties

        public SensorDataViewModel SensorDataViewModel
        {
            get => _sensorDataViewModel ?? (_sensorDataViewModel = new SensorDataViewModel(this));
            set => SetProperty(ref _sensorDataViewModel, value);
        }

        public ushort Relay1State
        {
            get => _relay1State;
            set => SetProperty(ref _relay1State, value);
        }

        #endregion

        #region Events

        private void OnRelayState1ParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter != null)
            {
                if(e.Parameter.GetValue(out ushort relay1State))
                {
                    Relay1State = relay1State;
                }
            }
        }

        #endregion

        #region Methods

        private async Task UpdateParameterValue()
        {
            ushort relay1State = 0;

            if (_relayState1Parameter != null)
            {
                var parameterValue = await Vcs.GetParameterValue(_relayState1Parameter.UniqueId);
                
                if(parameterValue.GetValue(ref relay1State))
                {
                    Relay1State = relay1State;
                }
            }
        }

        public override async Task Show()
        {
            if (Vcs.SearchParameter("PARAM_RelayState_1") is Parameter parameter)
            {
                _relayState1Parameter = parameter;

                _relayState1Parameter.ParameterChanged += OnRelayState1ParameterChanged;
            }

            await UpdateParameterValue();

            await base.Show();
        }

        public override async Task Hide()
        {
            if (_relayState1Parameter != null)
            {
                _relayState1Parameter.ParameterChanged -= OnRelayState1ParameterChanged;
            }

            await base.Hide();
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                if (Vcs != null)
                {
                    Vcs.RegisterParameterUpdate("PARAM_RelayState_1");
                }

                await UpdateParameterValue();
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                if (Vcs != null)
                {
                    Vcs.UnregisterParameterUpdate("PARAM_RelayState_1");
                }
            }

            return result;
        }

        #endregion
    }
}
