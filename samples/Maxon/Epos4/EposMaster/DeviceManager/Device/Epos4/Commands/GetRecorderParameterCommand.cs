using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetRecorderParameterCommand : DeviceCommand
	{
        public GetRecorderParameterCommand()
        { }
		
        public GetRecorderParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetRecorderParameter";

            AddParameter("SamplingPeriod", TypeCode.UInt16, ParameterType.Out);
            AddParameter("MaxNumberOfSamples", TypeCode.UInt16, ParameterType.Out);
            AddParameter("PrecedingSamples", TypeCode.UInt16, ParameterType.Out);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out GetRecorderParameterCommand result))
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
                
                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    var samplingPeriodParameter = obd.GetRecorderSamplingPeriodParameter();
                    var maxNumberOfSamplesParameter = obd.GetRecorderMaxNumberOfSamplesParameter();
                    var precedingSamplesParameter = obd.GetRecorderPrecedingSamplesParameter();

                    var data = new byte[samplingPeriodParameter.DataType.SizeInBytes];

                    commandResult = communication.GetObject(samplingPeriodParameter.Index,
                        samplingPeriodParameter.SubIndex,
                        ref data);

                    if (commandResult)
                    {
                        samplingPeriodParameter.SetValue(data);

                        data = new byte[maxNumberOfSamplesParameter.DataType.SizeInBytes];

                        commandResult = communication.GetObject(maxNumberOfSamplesParameter.Index,
                            maxNumberOfSamplesParameter.SubIndex,
                            ref data);
                    }

                    if (commandResult)
                    {
                        maxNumberOfSamplesParameter.SetValue(data);

                        data = new byte[precedingSamplesParameter.DataType.SizeInBytes];

                        commandResult = communication.GetObject(precedingSamplesParameter.Index,
                            precedingSamplesParameter.SubIndex,
                            ref data);

                        if (commandResult)
                        {
                            precedingSamplesParameter.SetValue(data);
                        }
                    }

                    if (commandResult)
                    {
                        samplingPeriodParameter.GetValue(out samplingPeriod);
                        maxNumberOfSamplesParameter.GetValue(out maxNumberOfSamples);
                        precedingSamplesParameter.GetValue(out precedingSamples);
                    }
                }
                
                SetParameterValue("SamplingPeriod", samplingPeriod);
                SetParameterValue("MaxNumberOfSamples", maxNumberOfSamples);
                SetParameterValue("PrecedingSamples", precedingSamples);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
