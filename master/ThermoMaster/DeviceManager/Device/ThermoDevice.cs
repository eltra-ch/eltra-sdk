using EltraCommon.Logger;
using ThermoMaster.DeviceManager.Device.Thermostat.Commands;
using System;
using ThermoMaster.Os.Linux;
using ThermoMaster.DeviceManager.Wrapper;
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

        protected override void OnPinsChanged()
        {
            try
            {
                if (SystemHelper.IsLinux)
                {
                    EltraRelayWrapper.Initialize();

                    foreach (var pin in Pins)
                    {
                        EltraRelayWrapper.RelayPinMode((ushort)pin, EltraRelayWrapper.GPIOpinmode.Output);
                    }
                }
                else
                {
                    MsgLogger.WriteLine(LogMsgType.Warning, "GPIO library is not supported on windows");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - OnPinsChanged", e);
            }
        }

        private void CreateCommandSet()
        {            
            //Object
            AddCommand(new GetChannelStateCommand(this));
            AddCommand(new SetChannelStateCommand(this));

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
