using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class ChannelSelection
    {
        #region Properties

        public string UniqueId { get; set; }

        public List<ChannelIdRef> ChannelIdRefs { get; set; }

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;

            ChannelIdRefs = new List<ChannelIdRef>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "channelIDRef")
                {
                    var channelIdRef = new ChannelIdRef();

                    if (!channelIdRef.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ChannelIdRefs.Add(channelIdRef);
                }
            }

            return result;
        }

        public void Resolve(List<Channel> channelsChannels)
        {
            foreach (var channelIdRef in ChannelIdRefs)
            {
                channelIdRef.Resolve(channelsChannels);
            }
        }

        public Parameter GetRecorderChannelParameter(byte channelNumber)
        {
            Parameter result = null;
            int channel = 1;

            foreach (var channelIdRef in ChannelIdRefs)
            {
                if (channel == channelNumber)
                {
                    result = channelIdRef?.Channel?.Parameter;
                    break;
                }
                channel++;
            }

            return result;
        }

        public byte GetRecorderChannelCount()
        {
            return (byte)ChannelIdRefs.Count;
        }

        #endregion
    }
}
