using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class RecorderTemplateList
    {
        public ChannelSelection ChannelSelection { get; set; }
        public TriggerCondition TriggerCondition { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "channelSelection")
                {
                    var channelSelection = new ChannelSelection();

                    if (!channelSelection.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ChannelSelection = channelSelection;
                }
                else if (childNode.Name == "triggerCondition")
                {
                    var triggerCondition = new TriggerCondition();

                    if (!triggerCondition.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerCondition = triggerCondition;
                }
            }

            return result;
        }

        public void Resolve(List<Channel> channelsChannels, XddParameterList parameterList)
        {
            ChannelSelection.Resolve(channelsChannels);

            TriggerCondition.Resolve(parameterList);
        }

        public Parameter GetRecorderChannelParameter(byte channelNumber)
        {
            Parameter result = null;

            if (ChannelSelection != null)
            {
                result = ChannelSelection.GetRecorderChannelParameter(channelNumber);
            }

            return result;
        }

        public byte GetRecorderChannelCount()
        {
            byte result = 0;

            if (ChannelSelection != null)
            {
                result = ChannelSelection.GetRecorderChannelCount();
            }

            return result;
        }

        public Parameter GetRecorderTriggerVariableParameter()
        {
            return TriggerCondition?.TriggerConditionParam?.Parameter;
        }

        public Parameter GetRecorderTriggerModeParameter()
        {
            return TriggerCondition?.TriggerConditionMode?.Parameter;
        }

        public Parameter GetRecorderTriggerHighValueParameter()
        {
            return TriggerCondition?.TriggerConditionHigh?.Parameter;
        }

        public Parameter GetRecorderTriggerLowValueParameter()
        {
            return TriggerCondition?.TriggerConditionLow?.Parameter;
        }

        public Parameter GetRecorderTriggerMaskParameter()
        {
            return TriggerCondition?.TriggerConditionMask?.Parameter;
        }
    }
}
