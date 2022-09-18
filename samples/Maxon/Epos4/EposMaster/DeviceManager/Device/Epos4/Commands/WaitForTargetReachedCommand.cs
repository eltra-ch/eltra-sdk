using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class WaitForTargetReachedCommand : DeviceCommand
	{
        public WaitForTargetReachedCommand()
        {
        }

		public WaitForTargetReachedCommand(EltraDevice device)
			:base(device)
		{
			Name = "WaitForTargetReached";

		    AddParameter("Timeout", TypeCode.UInt32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out WaitForTargetReachedCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint max = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Timeout", ref max);

		        var commandResult = communication.WaitForTargetReached(max);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
