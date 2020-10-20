using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetVelocityProfileCommand : DeviceCommand
	{
        public GetVelocityProfileCommand()
        {
        }

		public GetVelocityProfileCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityProfile";

		    AddParameter("Acceleration", TypeCode.UInt32, ParameterType.Out);
		    AddParameter("Deceleration", TypeCode.UInt32, ParameterType.Out);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetVelocityProfileCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint profileAcceleration = 0;
		    uint profileDeceleration = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetVelocityProfile(ref profileAcceleration, ref profileDeceleration);

		        SetParameterValue("Acceleration", profileAcceleration);
		        SetParameterValue("Deceleration", profileDeceleration);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
