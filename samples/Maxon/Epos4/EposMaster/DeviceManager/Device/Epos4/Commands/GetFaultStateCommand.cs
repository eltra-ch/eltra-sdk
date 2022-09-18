using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetFaultStateCommand : DeviceCommand
	{
        public GetFaultStateCommand()
        {
        }

		public GetFaultStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetFaultState";

            AddParameter("Fault", TypeCode.Boolean, ParameterType.Out);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);

        }

        public override DeviceCommand Clone()
        {
            Clone(out GetFaultStateCommand result);
            
            return result;
        }

        public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                var commandResult = communication.GetFaultState(out var fault);

                SetParameterValue("Fault", fault);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
