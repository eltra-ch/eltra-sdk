using System;
using EltraCommon.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetAnalogOutputStateCommand : DeviceCommand
	{
        public SetAnalogOutputStateCommand()
        { }
		
        public SetAnalogOutputStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetAnalogOutputState";

            AddParameter("Configuration", TypeCode.Int32);

            AddParameter("StateValue", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetAnalogOutputStateCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            int configuration = 0;
            int stateValue = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("Configuration", ref configuration);
                GetParameterValue("StateValue", ref stateValue);

                var commandResult = communication.SetAnalogOutputState((EAnalogOutputConfiguration)configuration, stateValue);
                
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
