using System;
using EltraCloudContracts.Contracts.CommandSets;
using EposMaster.DeviceManager.VcsWrapper;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetOperationModeCommand : DeviceCommand
	{
        public GetOperationModeCommand()
        {
        }

		public GetOperationModeCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "GetOperationMode";

		    AddParameter("Mode", TypeCode.SByte, ParameterType.Out);

		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetOperationModeCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    EOperationMode mode = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetOperationMode(ref mode);

		        SetParameterValue("Mode", (sbyte)mode);
                SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
