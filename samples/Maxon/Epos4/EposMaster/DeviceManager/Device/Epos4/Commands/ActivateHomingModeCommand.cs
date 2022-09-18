using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class ActivateHomingModeCommand : DeviceCommand
	{
        public ActivateHomingModeCommand()
        {
        }

		public ActivateHomingModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateHomingMode";

		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out ActivateHomingModeCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    var communication = eposDevice?.Communication as Epos4DeviceCommunication;

		    if (communication != null)
		    {
		        var commandResult = communication.ActivateHomingMode();

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
