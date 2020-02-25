using System;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class DataRecorder
    {
        #region Properties

        public string UniqueId { get; set; }

        public DataRecorderParameters Parameters { get; set; }

        public DataRecorderChannels Channels { get; set; }

        public RecorderTemplateList RecorderTemplateList { get; set; }

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "parameters")
                {
                    var parameters = new DataRecorderParameters();

                    if (!parameters.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Parameters = parameters;
                }
                else if (childNode.Name == "channels")
                {
                    var channels = new DataRecorderChannels();

                    if (!channels.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Channels = channels;
                }
                else if(childNode.Name == "recorderTemplateList")
                {
                    var recorderTemplateList = new RecorderTemplateList();

                    if (!recorderTemplateList.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    RecorderTemplateList = recorderTemplateList;
                }
            }

            return result;
        }

        public void ResolveParameterReferences(XddParameterList parameterList)
        {
            Parameters.Resolve(parameterList);

            Channels.Resolve(parameterList);

            RecorderTemplateList.Resolve(Channels.Channels, parameterList);
        }

        public Parameter GetChannelParameter(byte channelNumber)
        {
            Parameter result = null;

            if (RecorderTemplateList != null)
            {
                result = RecorderTemplateList.GetRecorderChannelParameter(channelNumber);
            }

            return result;
        }

        public byte GetChannelCount()
        {
            byte result = 0;

            if (RecorderTemplateList != null)
            {
                result = RecorderTemplateList.GetRecorderChannelCount();
            }

            return result;
        }

        public Parameter GetSamplingPeriodParameter()
        {
            return Parameters?.SamplingPeriod?.Parameter;
        }

        public Parameter GetMaxNumberOfSamplesParameter()
        {
            return Parameters?.MaxNbOfSamples.Parameter;
        }

        public Parameter GetPrecedingSamplesParameter()
        {
            return Parameters?.PrecedingSamples?.Parameter;
        }

        internal Parameter GetControlWordParameter()
        {
            return Parameters?.RecorderControlWord?.Parameter;
        }

        public Parameter GetStatusWordParameter()
        {
            return Parameters?.RecorderStatusWord?.Parameter;
        }

        public Parameter GetTriggerVariableParameter()
        {
            return RecorderTemplateList.GetRecorderTriggerVariableParameter();
        }

        public Parameter GetTriggerModeParameter()
        {
            return RecorderTemplateList.GetRecorderTriggerModeParameter();
        }

        public Parameter GetTriggerHighValueParameter()
        {
            return RecorderTemplateList.GetRecorderTriggerHighValueParameter();
        }

        public Parameter GetTriggerLowValueParameter()
        {
            return RecorderTemplateList.GetRecorderTriggerLowValueParameter();
        }

        public Parameter GetTriggerMaskParameter()
        {
            return RecorderTemplateList.GetRecorderTriggerMaskParameter();
        }

        public Parameter GetChannelBufferParameter(byte channelNumber)
        {
            return Channels.GetBufferParameter(channelNumber);
        }

        public Parameter GetTimestampParameter()
        {
            return Parameters?.BufferTimestamp?.Parameter;
        }

        public Parameter GetSamplesCountParameter()
        {
            return Parameters?.NbOfRecordedSamples?.Parameter;
        }

        #endregion
    }
}
