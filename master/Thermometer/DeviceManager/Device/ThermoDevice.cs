using ThermoMaster.DeviceManager.Device.Thermostat.Commands;
using ThermoMaster.DeviceManager.Device.Thermostat.Tools;
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
            CreateToolSet();
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
        
        private void CreateToolSet()
        {
            AddTool(new ThermoOverviewTool());
            AddTool(new ThermoSettingsTool());
        }

        #endregion
    }
}
