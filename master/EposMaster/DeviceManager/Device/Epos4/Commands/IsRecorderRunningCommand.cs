using System;
using System.Collections;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.ObjectDictionary.Epos4;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class IsRecorderRunningCommand : DeviceCommand
	{
        public IsRecorderRunningCommand()
        { }
		
        public IsRecorderRunningCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "IsRecorderRunning";

            AddParameter("Running", TypeCode.Boolean, ParameterType.Out);
            
            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out IsRecorderRunningCommand result);
            
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

                        var running = bitArray[0];
                        
                        SetParameterValue("Running", running);
                    }
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", result);
            }

            return result;
        }
	}
}
