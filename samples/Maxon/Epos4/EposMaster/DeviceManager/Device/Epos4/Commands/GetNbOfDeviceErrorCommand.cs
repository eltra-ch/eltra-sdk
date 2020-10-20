using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetNbOfDeviceErrorCommand : DeviceCommand
	{
        public GetNbOfDeviceErrorCommand()
        {
        }

		public GetNbOfDeviceErrorCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetNbOfDeviceError";

            //Result
		    AddParameter("NbOfDeviceErrors", TypeCode.Byte, ParameterType.Out);
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetNbOfDeviceErrorCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    byte numberOfDeviceError = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetNbOfDeviceError(ref numberOfDeviceError);

		        SetParameterValue("NbOfDeviceErrors", numberOfDeviceError);
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
