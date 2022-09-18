using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetHomingParameterCommand : DeviceCommand
	{
        public SetHomingParameterCommand()
        {
        }

		public SetHomingParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetHomingParameter";

            AddParameter("HomingAcceleration", TypeCode.UInt32);
		    AddParameter("SpeedSwitch", TypeCode.UInt32);
		    AddParameter("SpeedIndex", TypeCode.UInt32);
		    AddParameter("HomeOffset", TypeCode.Int32);
		    AddParameter("CurrentThreshold", TypeCode.UInt16);
		    AddParameter("HomePosition", TypeCode.Int32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetHomingParameterCommand result);
            
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
		        GetParameterValue("HomingAcceleration", ref homingAcceleration);
		        GetParameterValue("SpeedSwitch", ref speedSwitch);
		        GetParameterValue("SpeedIndex", ref speedIndex);
		        GetParameterValue("HomeOffset", ref homeOffset);
		        GetParameterValue("CurrentThreshold", ref currentThreshold);
		        GetParameterValue("HomePosition", ref homePosition);

                var commandResult = communication.SetHomingParameter(homingAcceleration, speedSwitch, speedIndex, homeOffset, currentThreshold, homePosition);
                
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
