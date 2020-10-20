using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class StopRecorderCommand : DeviceCommand
	{
        public StopRecorderCommand()
        { }
		
        public StopRecorderCommand(EltraDevice device)
			:base(device)
		{
			Name = "StopRecorder";

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out StopRecorderCommand result);
            
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
                    var controlWordParameter = obd.GetRecorderControlWordParameter();

                    short stopRecorder = 0;

                    result = communication.SetObject(controlWordParameter.Index, controlWordParameter.SubIndex, BitConverter.GetBytes(stopRecorder));
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", result);
            }

            return result;
        }
	}
}
