using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class DefinePositionCommand : DeviceCommand
	{
        public DefinePositionCommand()
        {
        }

		public DefinePositionCommand(EltraDevice device)
			:base(device)
		{
			Name = "DefinePosition";

		    AddParameter("Position", TypeCode.Int32);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out DefinePositionCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int position = 0;
			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Position", ref position);
		        
		        var commandResult = communication.DefinePosition(position);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
