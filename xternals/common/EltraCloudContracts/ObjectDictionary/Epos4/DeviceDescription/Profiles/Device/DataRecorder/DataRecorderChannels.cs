using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class DataRecorderChannels
    {
        public List<Channel> Channels { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            Channels = new List<Channel>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "channel")
                {
                    var channel = new Channel();

                    if (!channel.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Channels.Add(channel);
                }
            }

            return result;
        }

        public void Resolve(ParameterList parameterList)
        {
            foreach (var channel in Channels)
            {
                channel.Resolve(parameterList);
            }
        }

        private Channel FindChannel(byte channelNumber)
        {
            Channel result = null;
            byte index = 1;

            foreach (var channel in Channels)
            {
                if (index == channelNumber)
                {
                    result = channel;
                    break;
                }

                index++;
            }

            return result;
        }

        public Parameter GetBufferParameter(byte channelNumber)
        {
            Parameter result = null;
            var channel = FindChannel(channelNumber);

            if (channel != null)
            {
                result = channel.GetCompleteBufferParameter();
            }

            return result;
            ;
        }
    }
}
