using System;
using EltraCommon.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class FindHomeCommand : DeviceCommand
	{
        public FindHomeCommand()
        {
        }

		public FindHomeCommand(EltraDevice device)
			:base(device)
		{
			Name = "FindHome";

		    AddParameter("Method", TypeCode.Int32);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out FindHomeCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int method = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Method", ref method);

		        var commandResult = communication.FindHome((EHomingMethod)method);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
