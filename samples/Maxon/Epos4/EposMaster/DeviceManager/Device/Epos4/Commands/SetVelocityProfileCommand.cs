using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetVelocityProfileCommand : DeviceCommand
	{
        public SetVelocityProfileCommand()
        {
        }

		public SetVelocityProfileCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityProfile";
            
		    AddParameter("Acceleration", TypeCode.UInt32);
		    AddParameter("Deceleration", TypeCode.UInt32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetVelocityProfileCommand result);
            
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
		        GetParameterValue("Acceleration", ref profileAcceleration);
		        GetParameterValue("Deceleration", ref profileDeceleration);

		        var commandResult = communication.SetVelocityProfile(profileAcceleration, profileDeceleration);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
