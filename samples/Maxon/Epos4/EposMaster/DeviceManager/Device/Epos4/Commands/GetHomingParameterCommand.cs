using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetHomingParameterCommand : DeviceCommand
	{
        public GetHomingParameterCommand()
        {
        }

		public GetHomingParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetHomingParameter";

		    AddParameter("HomingAcceleration", TypeCode.UInt32, ParameterType.Out);
		    AddParameter("SpeedSwitch", TypeCode.UInt32, ParameterType.Out);
		    AddParameter("SpeedIndex", TypeCode.UInt32, ParameterType.Out);
		    AddParameter("HomeOffset", TypeCode.Int32, ParameterType.Out);
		    AddParameter("CurrentThreshold", TypeCode.UInt16, ParameterType.Out);
		    AddParameter("HomePosition", TypeCode.Int32, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetHomingParameterCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint homingAcceleration = 0;
            uint speedSwitch = 0;
            uint speedIndex = 0;
		    int homeOffset = 0;
		    ushort currentThreshold = 0;
		    int homePosition = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetHomingParameter(ref homingAcceleration, ref speedSwitch, ref speedIndex, ref homeOffset, ref currentThreshold, ref homePosition);

		        SetParameterValue("HomingAcceleration", homingAcceleration);
		        SetParameterValue("SpeedSwitch", speedSwitch);
		        SetParameterValue("SpeedIndex", speedIndex);
                SetParameterValue("HomeOffset", homeOffset);
		        SetParameterValue("CurrentThreshold", currentThreshold);
		        SetParameterValue("HomePosition", homePosition);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
