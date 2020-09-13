using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetMaxProfileVelocityCommand : DeviceCommand
	{
        public SetMaxProfileVelocityCommand()
        {
        }

		public SetMaxProfileVelocityCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetMaxProfileVelocity";

		    AddParameter("MaxProfileVelocity", TypeCode.UInt32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetMaxProfileVelocityCommand result);
            
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
		        GetParameterValue("MaxProfileVelocity", ref max);

                var commandResult = communication.SetMaxProfileVelocity(max);
                
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
