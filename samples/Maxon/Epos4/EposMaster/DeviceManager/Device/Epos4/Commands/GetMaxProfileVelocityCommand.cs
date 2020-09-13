using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetMaxProfileVelocityCommand : DeviceCommand
	{
        public GetMaxProfileVelocityCommand()
        {
        }

		public GetMaxProfileVelocityCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMaxProfileVelocity";

		    AddParameter("MaxProfileVelocity", TypeCode.UInt32, ParameterType.Out);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetMaxProfileVelocityCommand result);
            
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
		        var commandResult = communication.GetMaxProfileVelocity(ref max);

		        SetParameterValue("MaxProfileVelocity", max);
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
