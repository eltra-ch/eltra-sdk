using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels
{
    public class Channel
    {
        #region Private fields

        private List<Label> _labels;

        #endregion

        #region Properties

        public string UniqueId { get; set; }
        public List<Label> Labels => _labels ?? (_labels = new List<Label>());
        public Epos4ParamIdRef ParamIdRef { get; set; }
        public ChannelBuffer ChannelBuffer { get; set; }
        public Parameter Parameter { get; set; }
        
        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "label")
                {
                    var label = new Epos4Label(childNode);

                    if (!label.Parse())
                    {
                        result = false;
                        break;
                    }

                    Labels.Add(label);
                }
                else if (childNode.Name == "paramIDRef")
                {
                    var paramIdRef = new Epos4ParamIdRef();

                    if (!paramIdRef.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ParamIdRef = paramIdRef;
                }
                else if(childNode.Name == "channelBuffer")
                {
                    var channelBuffer = new ChannelBuffer();

                    if (!channelBuffer.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ChannelBuffer = channelBuffer;
                }
            }

            return result;
        }

        public void Resolve(ParameterList parameterList)
        {
            Parameter = parameterList.FindParameter(ParamIdRef.UniqueIdRef) as Parameter;

            ChannelBuffer.Resolve(parameterList);
        }
        public Parameter GetCompleteBufferParameter()
        {
            return ChannelBuffer.GetCompleteBufferParameter();
        }

        #endregion



    }
}
