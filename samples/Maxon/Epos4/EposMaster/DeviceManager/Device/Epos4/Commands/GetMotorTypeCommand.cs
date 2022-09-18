using System;
using EltraCommon.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetMotorTypeCommand : DeviceCommand
	{
        public GetMotorTypeCommand()
        {
        }

		public GetMotorTypeCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMotorType";

            AddParameter("MotorType", TypeCode.Int16, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetMotorTypeCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var communication = eposDevice?.Communication as Epos4DeviceCommunication;
            EMotorType motorType = 0;

            if (communication != null)
            {
                var commandResult = communication.GetMotorType(ref motorType);

                if (commandResult)
                {
                    SetParameterValue("MotorType", (short) motorType);
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
