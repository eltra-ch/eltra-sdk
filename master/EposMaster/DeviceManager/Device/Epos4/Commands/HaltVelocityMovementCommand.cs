using System;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class HaltVelocityMovementCommand : DeviceCommand
	{
        public HaltVelocityMovementCommand()
        {
        }

		public HaltVelocityMovementCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "HaltVelocityMovement";

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out HaltVelocityMovementCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                var commandResult = communication.HaltVelocityMovement();

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
