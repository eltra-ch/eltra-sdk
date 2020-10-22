using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.VcsWrapper;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetOperationModeCommand : DeviceCommand
	{
        public SetOperationModeCommand()
        {
        }

		public SetOperationModeCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetOperationMode";

		    AddParameter("Mode", TypeCode.SByte);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetOperationModeCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    sbyte mode = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("Mode", ref mode);

                var commandResult = communication.SetOperationMode((EOperationMode)mode);

		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
