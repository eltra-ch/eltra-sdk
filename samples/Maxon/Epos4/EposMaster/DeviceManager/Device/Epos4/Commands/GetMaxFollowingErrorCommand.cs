using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetMaxFollowingErrorCommand : DeviceCommand
	{
        public GetMaxFollowingErrorCommand()
        {
        }

		public GetMaxFollowingErrorCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMaxFollowingError";

		    AddParameter("MaxFollowingError", TypeCode.UInt32, ParameterType.Out);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetMaxFollowingErrorCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint max = 0;
			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetMaxFollowingError(ref max);

		        SetParameterValue("MaxFollowingError", max);
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
