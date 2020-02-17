using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EltraCloudContracts.Contracts.DataRecorder
{
    [DataContract]
    public class DataRecorderSamples
    {
        private List<DataRecorderSample> _samples;
        
        [DataMember]
        public byte ChannelNumber { get; set; }

        [DataMember]
        public List<DataRecorderSample> Samples { get => _samples ?? (_samples = new List<DataRecorderSample>()); }

        [DataMember]
        public ushort SamplingPeriod { get; set; }

        [DataMember]
        public uint LastTimestamp { get; set; }

        public bool CreateSamples(byte[] data, ushort size)
        {
            bool result = false;

            if (data.Length >= sizeof(int) * size)
            {
                uint samplingPeriodInUs = (uint)10 * SamplingPeriod;
                uint timestamp = (uint)Math.Abs(LastTimestamp - (double)(size * samplingPeriodInUs));
                
                for (int i = 0; i < size; i++)
                {
                    var sample = new DataRecorderSample {Timestamp = timestamp};

                    sample.Value = BitConverter.ToInt32(data.Skip(i*sizeof(int)).Take(sizeof(int)).ToArray(),0);

                    Samples.Add(sample);

                    timestamp += samplingPeriodInUs;
                }

                result = true;
            }

            return result;
        }
    }
}
