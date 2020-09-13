using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class ActivateCurrentModeCommand : DeviceCommand
	{
        public ActivateCurrentModeCommand()
        {
        }

		public ActivateCurrentModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateCurrentMode";

		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out ActivateCurrentModeCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    var communication = eposDevice?.Communication as Epos4DeviceCommunication;

		    if (communication != null)
		    {
		        var commandResult = communication.ActivateCurrentMode();

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
