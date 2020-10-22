using System;
using EltraCommon.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetMotorTypeCommand : DeviceCommand
	{
        public SetMotorTypeCommand()
        {
        }

		public SetMotorTypeCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetMotorType";

            AddParameter("MotorType", TypeCode.Int16);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetMotorTypeCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            short motorType = 0;

            var deviceCommunication = eposDevice?.Communication;

            if (deviceCommunication is Epos4DeviceCommunication communication)
            {
                GetParameterValue("MotorType", ref motorType);

                var commandResult = communication.SetMotorType((EMotorType)motorType);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
