using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetErrorInfoCommand : DeviceCommand
	{
        public GetErrorInfoCommand()
        {
        }

		public GetErrorInfoCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetErrorInfo";

		    AddParameter("ErrorCode", TypeCode.UInt32);

		    AddParameter("ErrorInfo", TypeCode.String);
		    AddParameter("Result", TypeCode.Boolean);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetErrorInfoCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    string errorInfo = string.Empty;
		    uint errorCode = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("ErrorCode", ref errorCode);
		        
                var commandResult = communication.GetErrorInfo(errorCode, ref errorInfo);

		        SetParameterValue("ErrorInfo", errorInfo);
                SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
