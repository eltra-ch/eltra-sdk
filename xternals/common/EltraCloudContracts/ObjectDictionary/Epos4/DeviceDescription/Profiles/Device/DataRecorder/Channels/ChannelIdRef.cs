using System.Collections.Generic;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels
{
    public class ChannelIdRef
    {
        #region Private fields

        public string UniqueIdRef { get; set; }
        public Channel Channel { get; set; }

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = false;

            if (node != null)
            {
                UniqueIdRef = node.Attributes["uniqueIDRef"].InnerXml;

                result = true;
            }

            return result;
        }

        public void Resolve(List<Channel> channelsChannels)
        {
            foreach (var channel in channelsChannels)
            {
                if (channel.UniqueId == UniqueIdRef)
                {
                    Channel = channel;
                    break;
                }
            }
        }

        #endregion
    }
}
