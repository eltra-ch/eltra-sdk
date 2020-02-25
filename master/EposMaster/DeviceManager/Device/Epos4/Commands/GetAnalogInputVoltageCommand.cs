using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetAnalogInputVoltageCommand : DeviceCommand
	{
        public GetAnalogInputVoltageCommand()
        { }
		
        public GetAnalogInputVoltageCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetAnalogInputVoltage";

            AddParameter("InputNumber", TypeCode.UInt16);

            AddParameter("VoltageValue", TypeCode.Int32, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetAnalogInputVoltageCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            ushort inputNumber = 0;
            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("InputNumber", ref inputNumber);

                var commandResult = communication.GetAnalogInputVoltage(inputNumber, out var voltageValue);

                SetParameterValue("VoltageValue", voltageValue);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
