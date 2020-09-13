using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class MoveToPositionCommand : DeviceCommand
	{
        public MoveToPositionCommand()
        {
        }

		public MoveToPositionCommand(EltraDevice device)
			:base(device)
		{
			Name = "MoveToPosition";

		    AddParameter("Position", TypeCode.Int32);
		    AddParameter("Absolute", TypeCode.Boolean);
		    AddParameter("Immediately", TypeCode.Boolean);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out MoveToPositionCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    int position = 0;
		    bool immediately = false;
		    bool absolute = false;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Position", ref position);
		        GetParameterValue("Absolute", ref absolute);
		        GetParameterValue("Immediately", ref immediately);

                var commandResult = communication.MoveToPosition(position, absolute, immediately);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
