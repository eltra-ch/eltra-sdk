using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetPositionProfileCommand : DeviceCommand
	{
        public SetPositionProfileCommand()
        {
        }

		public SetPositionProfileCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionProfile";

		    AddParameter("Velocity", TypeCode.UInt32);
		    AddParameter("Acceleration", TypeCode.UInt32);
		    AddParameter("Deceleration", TypeCode.UInt32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetPositionProfileCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint profileVelocity = 0;
		    uint profileAcceleration = 0;
		    uint profileDeceleration = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Velocity", ref profileVelocity);
		        GetParameterValue("Acceleration", ref profileAcceleration);
		        GetParameterValue("Deceleration", ref profileDeceleration);

                var commandResult = communication.SetPositionProfile(profileVelocity, profileAcceleration, profileDeceleration);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
