using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetAllDigitalOutputsCommand : DeviceCommand
	{
        public SetAllDigitalOutputsCommand()
        { }
		
        public SetAllDigitalOutputsCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "SetAllDigitalOutputs";

            AddParameter("DigitalOutputs", TypeCode.UInt16);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetAllDigitalOutputsCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            ushort digitalOutputs = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("DigitalOutputs", ref digitalOutputs);

                var commandResult = communication.SetAllDigitalOutputs(digitalOutputs);
                
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
