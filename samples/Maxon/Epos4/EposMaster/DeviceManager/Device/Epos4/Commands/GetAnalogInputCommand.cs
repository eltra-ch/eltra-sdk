using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetAnalogInputCommand : DeviceCommand
	{
        public GetAnalogInputCommand()
        { }
		
        public GetAnalogInputCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetAnalogInput";

            AddParameter("InputNumber", TypeCode.UInt16);

            AddParameter("AnalogValue", TypeCode.UInt16, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetAnalogInputCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            ushort inputNumber = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("InputNumber", ref inputNumber);

                var commandResult = communication.GetAnalogInput(inputNumber, out var analogValue);

                SetParameterValue("AnalogValue", analogValue);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
