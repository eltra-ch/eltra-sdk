using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetQuickStopStateCommand : DeviceCommand
	{
        public GetQuickStopStateCommand()
        {
        }

		public GetQuickStopStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetQuickStopState";

		    AddParameter("State", TypeCode.Boolean, ParameterType.Out);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetQuickStopStateCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int isQuickStopped = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetQuickStopState(ref isQuickStopped);

		        SetParameterValue("State", isQuickStopped > 0);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
