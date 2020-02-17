using EltraNavigo.Views.Devices.Thermo.Base;
using EltraNavigo.Controls.Parameters;
using System.Threading.Tasks;
using EltraNavigo.Controls;

namespace EltraNavigo.Views.Devices.Thermo.Control
{
    public class BasicSensorDataViewModel : ThermoToolViewModel
    {
        #region Private fields

        private ParameterLabelViewModel _temperature;
        private ParameterLabelViewModel _humidity;
        
		#endregion

        #region Constructors

        public BasicSensorDataViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _temperature = new ParameterLabelViewModel(this, "PARAM_ActualTemperature");
            _humidity = new ParameterLabelViewModel(this, "PARAM_ActualHumidity");
        }

        #endregion

        #region Properties

        public ParameterLabelViewModel Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public ParameterLabelViewModel Humidity
        {
            get => _humidity;
            set => SetProperty(ref _humidity, value);
        }

        #endregion
        
        #region Methods

        public override async Task Show()
        {
            IsBusy = true;

            Temperature.InitModelData();

            Humidity.InitModelData();
			
            await base.Show();

            IsBusy = false;
        }

        #endregion
    }
}
