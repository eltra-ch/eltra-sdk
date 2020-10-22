using System;
using System.Collections;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class IsRecorderTriggeredCommand : DeviceCommand
	{
        public IsRecorderTriggeredCommand()
        { }
		
        public IsRecorderTriggeredCommand(EltraDevice device)
			:base(device)
		{
			Name = "IsRecorderTriggered";

            AddParameter("Triggered", TypeCode.Boolean, ParameterType.Out);
           
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out IsRecorderRunningCommand result))
            {
                result.Device = Device;
            }

            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var communication = eposDevice?.Communication;

            if (communication != null)
            {
                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    var statusWordParameter = obd.GetRecorderStatusWordParameter();
                    short statusWord = 0;
                    byte[] data = BitConverter.GetBytes(statusWord);

                    result = communication.GetObject(statusWordParameter.Index, statusWordParameter.SubIndex, ref data);

                    if (result)
                    {
                        var bitArray = new BitArray(data);

                        var triggered = bitArray[1];
                        
                        SetParameterValue("Triggered", triggered);
                    }
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", result);
            }

            return result;
        }
	}
}
