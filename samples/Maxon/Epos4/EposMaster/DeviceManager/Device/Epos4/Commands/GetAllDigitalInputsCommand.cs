using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetAllDigitalInputsCommand : DeviceCommand
	{
        public GetAllDigitalInputsCommand()
        { }
		
        public GetAllDigitalInputsCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetAllDigitalInputs";

            AddParameter("DigitalInputs", TypeCode.UInt16, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetAllDigitalInputsCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                var commandResult = communication.GetAllDigitalInputs(out var digitalInputs);

                SetParameterValue("DigitalInputs", digitalInputs);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
