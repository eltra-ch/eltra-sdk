using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class WaitForHomingAttainedCommand : DeviceCommand
	{
        public WaitForHomingAttainedCommand()
        {
        }

		public WaitForHomingAttainedCommand(EltraDevice device)
			:base(device)
		{
			Name = "WaitForHomingAttained";

		    AddParameter("Timeout", TypeCode.UInt32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out WaitForHomingAttainedCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint timeout = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Timeout", ref timeout);

		        var commandResult = communication.WaitForHomingAttained(timeout);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
