using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetAnalogOutputVoltageCommand : DeviceCommand
	{
        public SetAnalogOutputVoltageCommand()
        { }
		
        public SetAnalogOutputVoltageCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetAnalogOutputVoltage";

            AddParameter("OutputNumber", TypeCode.UInt16);

            AddParameter("VoltageValue", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetAnalogOutputVoltageCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            ushort outputNumber = 0;
            int voltageValue = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("OutputNumber", ref outputNumber);
                GetParameterValue("VoltageValue", ref voltageValue);

                var commandResult = communication.SetAnalogOutputVoltage(outputNumber, voltageValue);
                
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
