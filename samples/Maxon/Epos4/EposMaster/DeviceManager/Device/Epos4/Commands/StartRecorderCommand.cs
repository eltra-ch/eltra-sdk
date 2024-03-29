using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class StartRecorderCommand : DeviceCommand
	{
        public StartRecorderCommand()
        { }
		
        public StartRecorderCommand(EltraDevice device)
			:base(device)
		{
			Name = "StartRecorder";

            AddParameter("ForceTrigger", TypeCode.Boolean);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            Clone(out StartRecorderCommand result);
            
            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
            bool result = false;
            var eposDevice = Device as EposDevice;
            var communication = eposDevice?.Communication;

            if (communication != null)
            {
                bool forceTrigger = false;
                short stopRecorder = 0;
                short startRecorder = 1;

                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    var controlWordParameter = obd.GetRecorderControlWordParameter();

                    GetParameterValue("", ref forceTrigger);

                    result = communication.SetObject(controlWordParameter.Index, controlWordParameter.SubIndex,
                        BitConverter.GetBytes(stopRecorder));

                    if (forceTrigger)
                    {
                        startRecorder = 3;
                    }

                    if (result)
                    {
                        result = communication.SetObject(controlWordParameter.Index, controlWordParameter.SubIndex,
                            BitConverter.GetBytes(startRecorder));
                    }
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", result);
            }

            return result;
        }
	}
}
