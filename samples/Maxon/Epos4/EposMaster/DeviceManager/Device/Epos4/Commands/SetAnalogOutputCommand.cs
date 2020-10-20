using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetAnalogOutputCommand : DeviceCommand
	{
        public SetAnalogOutputCommand()
        { }
		
        public SetAnalogOutputCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetAnalogOutput";

            AddParameter("OutputNumber", TypeCode.UInt16);

            AddParameter("AnalogValue", TypeCode.UInt16);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetAnalogOutputCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            ushort inputNumber = 0;
            ushort analogValue = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("OutputNumber", ref inputNumber);
                GetParameterValue("AnalogValue", ref analogValue);

                var commandResult = communication.SetAnalogOutput(inputNumber, analogValue);
                
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
