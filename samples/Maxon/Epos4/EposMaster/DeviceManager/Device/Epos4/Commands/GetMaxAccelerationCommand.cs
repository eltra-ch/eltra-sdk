using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetMaxAccelerationCommand : DeviceCommand
	{
        public GetMaxAccelerationCommand()
        {
        }

		public GetMaxAccelerationCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMaxAcceleration";

		    AddParameter("MaxAcceleration", TypeCode.UInt32, ParameterType.Out);

            //Result
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out GetMaxAccelerationCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    uint max = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        var commandResult = communication.GetMaxAcceleration(ref max);

		        SetParameterValue("MaxAcceleration", max);
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
