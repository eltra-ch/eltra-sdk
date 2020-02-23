using System;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Parameters;

namespace Thermometer.DeviceManager.Device.Commands
{
    public class RegisterParameterUpdateCommand : DeviceCommand
    {
        public RegisterParameterUpdateCommand()
        { }
        
        public RegisterParameterUpdateCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {
            Name = "RegisterParameterUpdate";

            //In
            AddParameter("Index",TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);
            AddParameter("Priority", TypeCode.Int32);
            
            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out RegisterParameterUpdateCommand result);
            
            return result;
        }
        
        public override bool Execute(string source)
        {
            bool result = false;
            var relayDevice = Device as ThermoDevice;
            var communication = relayDevice?.Communication;
            ushort index = 0;
            byte subIndex = 0;
            int priority = 0;

            GetParameterValue("Index", ref index);
            GetParameterValue("SubIndex", ref subIndex);
            GetParameterValue("Priority", ref priority);
                        
            if (communication != null)
            {
                var manager = relayDevice.ParameterConnectionManager;

                bool commandResult = false;

                if (manager != null)
                {
                    commandResult = manager.RegisterParameter(source, index, subIndex, (ParameterUpdatePriority)priority);
                }

                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
    }
}
