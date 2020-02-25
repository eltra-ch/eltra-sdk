using EltraNavigo.Views.Devices.Thermo.Base;
using EltraNavigo.Controls.Parameters;
using System.Threading.Tasks;
using EltraNavigo.Controls;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;

namespace EltraNavigo.Views.Devices.Thermo.Overview
{
    public class SensorDataViewModel : ThermoToolViewModel
    {
        #region Private fields

        private ParameterLabelViewModel _actualTemperature;
        private ParameterLabelViewModel _actualTemperatureExt;
        private ParameterLabelViewModel _temperatureTimestamp;
        private ParameterLabelViewModel _temperatureTimestampExt;

        private ParameterLabelViewModel _actualHumidity;
        private ParameterLabelViewModel _actualPressure;
        private ParameterLabelViewModel _humidityTimestamp;
        
        private ParameterLabelViewModel _actualTimestamp;

        #endregion

        #region Constructors

        public SensorDataViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _actualTemperature = new ParameterLabelViewModel(this, "PARAM_ActualTemperature");
            _actualTemperatureExt = new ParameterLabelViewModel(this, "PARAM_ActualExternalTemperature");

            _temperatureTimestamp = new ParameterLabelViewModel(this, "PARAM_TemperatureTimestamp") { ShowLabel = false };
            _temperatureTimestampExt = new ParameterLabelViewModel(this, "PARAM_ExternalTemperatureTimestamp") { ShowLabel = false };

            _actualHumidity = new ParameterLabelViewModel(this, "PARAM_ActualHumidity");
            _actualPressure = new ParameterLabelViewModel(this, "PARAM_Pressure");

            _humidityTimestamp = new ParameterLabelViewModel(this, "PARAM_HumidityTimestamp") { ShowLabel = false };
            
            var dateTimeFormat = "MM/dd HH:mm";

            _temperatureTimestamp.DateTimeFormat = dateTimeFormat;
            _temperatureTimestampExt.DateTimeFormat = dateTimeFormat;
            _humidityTimestamp.DateTimeFormat = dateTimeFormat;
        }

        #endregion

        #region Properties

        public ParameterLabelViewModel ActualTemperature
        {
            get => _actualTemperature;
            set => SetProperty(ref _actualTemperature, value);
        }

        public ParameterLabelViewModel ActualTemperatureExt
        {
            get => _actualTemperatureExt;
            set => SetProperty(ref _actualTemperatureExt, value);
        }

        public ParameterLabelViewModel TemperatureTimestamp
        {
            get => _temperatureTimestamp;
            set => SetProperty(ref _temperatureTimestamp, value);
        }

        public ParameterLabelViewModel TemperatureTimestampExt
        {
            get => _temperatureTimestampExt;
            set => SetProperty(ref _temperatureTimestampExt, value);
        }

        public ParameterLabelViewModel ActualHumidity
        {
            get => _actualHumidity;
            set => SetProperty(ref _actualHumidity, value);
        }

        public ParameterLabelViewModel ActualPressure
        {
            get => _actualPressure;
            set => SetProperty(ref _actualPressure, value);
        }

        public ParameterLabelViewModel HumidityTimestamp
        {
            get => _humidityTimestamp;
            set => SetProperty(ref _humidityTimestamp, value);
        }

        public ParameterLabelViewModel ActualTimestamp
        {
            get => _actualTimestamp;
            set => SetProperty(ref _actualTimestamp, value);
        }

        #endregion

        #region Events
        
        private void OnHumidityChanged(object sender, ParameterChangedEventArgs e)
        {
            ActualTimestamp = HumidityTimestamp;
        }

        private void OnTemperatureChanged(object sender, ParameterChangedEventArgs e)
        {
            ActualTimestamp = TemperatureTimestamp;    
        }

        private void OnTemperatureExtChanged(object sender, ParameterChangedEventArgs e)
        {
            ActualTimestamp = TemperatureTimestampExt;
        }

        #endregion

        #region Methods

        public override async Task Show()
        {
            IsBusy = true;

            await Task.Run(() => { 
                ActualTemperature.InitModelData();
                ActualTemperatureExt.InitModelData();
            
                TemperatureTimestamp.InitModelData();
                TemperatureTimestampExt.InitModelData();

                ActualHumidity.InitModelData();
                ActualPressure.InitModelData();

                HumidityTimestamp.InitModelData();

                ActualTimestamp = HumidityTimestamp;
            });

            _temperatureTimestamp.Model.ParameterChanged += OnTemperatureChanged;
            _temperatureTimestampExt.Model.ParameterChanged += OnTemperatureExtChanged;
            _humidityTimestamp.Model.ParameterChanged += OnHumidityChanged;

            await base.Show();

            IsBusy = false;
        }

        public override Task Hide()
        {
            _temperatureTimestamp.Model.ParameterChanged -= OnTemperatureChanged;
            _temperatureTimestampExt.Model.ParameterChanged -= OnTemperatureExtChanged;
            _humidityTimestamp.Model.ParameterChanged -= OnHumidityChanged;

            return base.Hide();
        }

        #endregion
    }
}
