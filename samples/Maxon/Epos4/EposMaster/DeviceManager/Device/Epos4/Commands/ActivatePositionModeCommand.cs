using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class ActivatePositionModeCommand : DeviceCommand
	{
        public ActivatePositionModeCommand()
        {
        }

		public ActivatePositionModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivatePositionMode";

		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out ActivatePositionModeCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    var deviceCommunication = eposDevice?.Communication;

		    if (deviceCommunication is Epos4DeviceCommunication communication)
		    {
		        var commandResult = communication.ActivatePositionMode();

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
