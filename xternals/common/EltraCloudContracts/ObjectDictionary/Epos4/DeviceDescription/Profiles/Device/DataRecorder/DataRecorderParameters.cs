using System;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class DataRecorderParameters
    {
        #region Properties

        public MaxNbOfSamples MaxNbOfSamples { get; set; }

        public NbOfRecordedSamples NbOfRecordedSamples { get; set; }

        public PrecedingSamples PrecedingSamples { get; set; }

        public RecorderControlWord RecorderControlWord { get; set; }
        public RecorderSnapshot RecorderSnapshot { get; set; }
        public RecorderStatusWord RecorderStatusWord { get; set; }
        public SamplingPeriod SamplingPeriod { get; set; }
        public BufferTimestamp BufferTimestamp { get; set; }

        #endregion
        
        #region Methods
        
        public bool Parse(XmlNode node)
        {
            bool result = true;
            
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "paramSamplingPeriod")
                {
                    var parameter = new SamplingPeriod();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    SamplingPeriod = parameter;
                }
                else if (childNode.Name == "paramPrecedingSamples")
                {
                    var parameter = new PrecedingSamples();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    PrecedingSamples = parameter;
                }
                else if (childNode.Name == "paramMaxNbOfSamples")
                {
                    var parameter = new MaxNbOfSamples();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    MaxNbOfSamples = parameter;
                }
                else if (childNode.Name == "paramNbOfRecordedSamples")
                {
                    var parameter = new NbOfRecordedSamples();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    NbOfRecordedSamples = parameter;
                }
                else if (childNode.Name == "paramRecorderSnapshot")
                {
                    var parameter = new RecorderSnapshot();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    RecorderSnapshot = parameter;
                }
                else if (childNode.Name == "paramRecorderControlWord")
                {
                    var parameter = new RecorderControlWord();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    RecorderControlWord = parameter;
                }
                else if (childNode.Name == "paramRecorderStatusWord")
                {
                    var parameter = new RecorderStatusWord();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    RecorderStatusWord = parameter;
                }
                else if (childNode.Name == "paramBufferTimestamp")
                {
                    var parameter = new BufferTimestamp();

                    if (!parameter.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    BufferTimestamp = parameter;
                }
            }

            return result;
        }
        public void Resolve(XddParameterList parameterList)
        {
            MaxNbOfSamples.Resolve(parameterList);
            NbOfRecordedSamples.Resolve(parameterList);
            PrecedingSamples.Resolve(parameterList);
            RecorderControlWord.Resolve(parameterList);
            RecorderStatusWord.Resolve(parameterList);
            SamplingPeriod.Resolve(parameterList);
            RecorderSnapshot.Resolve(parameterList);
            BufferTimestamp.Resolve(parameterList);
        }

        #endregion
    }
}
