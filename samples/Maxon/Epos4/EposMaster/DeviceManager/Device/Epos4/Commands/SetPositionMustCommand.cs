using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetPositionMustCommand : DeviceCommand
	{
        public SetPositionMustCommand()
        {
        }

		public SetPositionMustCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionMust";

		    AddParameter("Position", TypeCode.Int32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetPositionMustCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int must = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Position", ref must);

		        var commandResult = communication.SetPositionMust(must);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
