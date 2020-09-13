using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.VcsWrapper;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetStateCommand : DeviceCommand
	{
        public SetStateCommand()
        {
        }

		public SetStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetState";

		    AddParameter("State", TypeCode.UInt16);

		    //Result
		    AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
		    AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out SetStateCommand result);
            
            return result;
        }

		public override bool Execute(string source)
		{
		    bool result = false;
		    var eposDevice = Device as EposDevice;
		    ushort state = 0;

			var deviceCommunication = eposDevice?.Communication;

			if (deviceCommunication is Epos4DeviceCommunication communication)
			{
		        GetParameterValue("State", ref state);

                var commandResult = communication.SetState((EStates)state);
                
		        SetParameterValue("ErrorCode", communication.LastErrorCode);
		        SetParameterValue("Result", commandResult);

		        result = true;
		    }

		    return result;
        }
	}
}
