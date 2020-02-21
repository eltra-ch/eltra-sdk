using ThermoMaster.DeviceManager.Device.Thermostat.Commands;
using ThermoMaster.Settings;

namespace ThermoMaster.DeviceManager.Device
{
    sealed class ThermoDevice : ThermoDeviceBase
    {
        #region Constructors

        public ThermoDevice(MasterSettings settings)
            : base("THERMO", settings)
        {
            CreateCommandSet();
        }

        #endregion

        #region Methods

        private void CreateCommandSet()
        {            
            AddCommand(new GetTemperatureCommand(this));
            AddCommand(new GetHumidityCommand(this));

            AddCommand(new RegisterParameterUpdateCommand(this));
            AddCommand(new UnregisterParameterUpdateCommand(this));

            AddCommand(new GetObjectCommand(this));
            AddCommand(new SetObjectCommand(this));
        }
        
        #endregion
    }
}
