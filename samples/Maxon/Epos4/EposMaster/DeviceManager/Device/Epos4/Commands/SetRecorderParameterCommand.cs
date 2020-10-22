using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetRecorderParameterCommand : DeviceCommand
	{
        public SetRecorderParameterCommand()
        { }
		
        public SetRecorderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetRecorderParameter";

            AddParameter("SamplingPeriod", TypeCode.UInt16);
            AddParameter("MaxNumberOfSamples", TypeCode.UInt16);
            AddParameter("PrecedingSamples", TypeCode.UInt16);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out SetRecorderParameterCommand result))
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
            bool commandResult = false;

            if (communication != null)
            {
                ushort samplingPeriod = 0;
                ushort maxNumberOfSamples = 0;
                ushort precedingSamples = 0;

                GetParameterValue("SamplingPeriod", ref samplingPeriod);
                GetParameterValue("MaxNumberOfSamples", ref maxNumberOfSamples);
                GetParameterValue("PrecedingSamples", ref precedingSamples);

                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    var samplingPeriodParameter = obd.GetRecorderSamplingPeriodParameter();
                    
                    commandResult = communication.SetObject(samplingPeriodParameter.Index,
                        samplingPeriodParameter.SubIndex,
                        BitConverter.GetBytes(samplingPeriod));

                    if (commandResult)
                    {
                        var maxNumberOfSamplesParameter = obd.GetRecorderMaxNumberOfSamplesParameter();

                        commandResult = communication.SetObject(maxNumberOfSamplesParameter.Index,
                            maxNumberOfSamplesParameter.SubIndex,
                            BitConverter.GetBytes(maxNumberOfSamples));
                    }

                    if (commandResult)
                    {
                        var precedingSamplesParameter = obd.GetRecorderPrecedingSamplesParameter();

                        commandResult = communication.SetObject(precedingSamplesParameter.Index,
                            precedingSamplesParameter.SubIndex,
                            BitConverter.GetBytes(precedingSamples));
                    }
                }

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
