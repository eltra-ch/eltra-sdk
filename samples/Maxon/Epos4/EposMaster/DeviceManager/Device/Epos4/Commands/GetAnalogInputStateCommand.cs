using System;
using EltraCommon.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetAnalogInputStateCommand : DeviceCommand
	{
        public GetAnalogInputStateCommand()
        { }
		
        public GetAnalogInputStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetAnalogInputState";

            AddParameter("Configuration", TypeCode.Int32);

            AddParameter("StateValue", TypeCode.Int32, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetAnalogInputStateCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            int configuration = 0;
            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            { 
                GetParameterValue("Configuration", ref configuration);
                
                var commandResult = communication.GetAnalogInputState((EAnalogInputConfiguration)configuration, out var voltageValue);

                SetParameterValue("StateValue", voltageValue);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
