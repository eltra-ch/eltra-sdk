using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.DataRecorder;
using EltraCommon.ObjectDictionary.Epos4;
using Newtonsoft.Json;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class ReadChannelDataVectorCommand : DeviceCommand
	{
        public ReadChannelDataVectorCommand()
        { }
        
        public ReadChannelDataVectorCommand(EltraDevice device)
			:base(device)
		{
			Name = "ReadChannelDataVector";

            AddParameter("Channel", TypeCode.Byte);
            AddParameter("Samples", TypeCode.String, ParameterType.Out);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out ReadChannelDataVectorCommand result))
            {
                result.Device = Device;
            }

            return result;
        }

		public override bool Execute(string source)
        {
            bool result = false;
            uint errorCode = 0;
            byte channelNumber = 0;
            var eposDevice = Device as Epos4Device;
            var communication = eposDevice?.Communication;
            
            if (communication != null)
            {
                GetParameterValue("Channel", ref channelNumber);

                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary objectDictionary)
                {
                    var dataRecorderSamples = new DataRecorderSamples { ChannelNumber = channelNumber };

                    var channelBufferParameter = objectDictionary.GetRecorderChannelBufferParameter(channelNumber);
                    var timestampParameter = objectDictionary.GetRecorderBufferTimestampParameter();
                    var recordedSamplesParameter = objectDictionary.GetRecorderSamplesCountParameter();
                    var samplingPeriodParameter = objectDictionary.GetRecorderSamplingPeriodParameter();

                    ushort samplingPeriod = 0;
                    byte[] data = null;
                    uint lastTimestamp = 0;

                    if (channelBufferParameter != null && timestampParameter != null && recordedSamplesParameter!=null)
                    {
                        result = eposDevice.ReadParameterValue(recordedSamplesParameter, out ushort recordedSamples);

                        if (result)
                        {
                            result = eposDevice.ReadParameterValue(timestampParameter, out lastTimestamp);
                        }

                        if (result)
                        {
                            result = eposDevice.ReadParameterValue(channelBufferParameter, out data);
                        }

                        if (result)
                        {
                            result = eposDevice.ReadParameterValue(samplingPeriodParameter, out samplingPeriod);
                        }

                        if(result)
                        {
                            dataRecorderSamples.SamplingPeriod = samplingPeriod;
                            dataRecorderSamples.LastTimestamp = lastTimestamp;

                            result = dataRecorderSamples.CreateSamples(data, recordedSamples);

                            var json = JsonConvert.SerializeObject(dataRecorderSamples);

                            SetParameterValue("Samples", json);
                        }
                    }
                }
            }

            SetParameterValue("Result", result);
            SetParameterValue("ErrorCode", errorCode);

            return result;
		}
	}
}
