using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class MoveWithVelocityCommand : DeviceCommand
	{
        public MoveWithVelocityCommand()
        {
        }

		public MoveWithVelocityCommand(EltraDevice device)
			:base(device)
		{
			Name = "MoveWithVelocity";

		    AddParameter("Velocity", TypeCode.Int32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out MoveWithVelocityCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int velocity = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Velocity", ref velocity);

                var commandResult = communication.MoveWithVelocity(velocity);
                
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
