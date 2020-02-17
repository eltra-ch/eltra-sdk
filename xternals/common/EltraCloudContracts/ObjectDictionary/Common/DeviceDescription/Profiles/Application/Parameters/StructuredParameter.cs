using EltraCloudContracts.Contracts.Devices;
using System;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters
{
    public class StructuredParameter : ParameterBase
    {
        public StructuredParameter(EltraDevice device, XmlNode source) 
            : base(device, source)
        {
        }

        #region Methods

        public override bool Parse()
        {
            bool result = false;

            UniqueId = Source.Attributes["uniqueID"].InnerXml;

            foreach (XmlNode childNode in Source.ChildNodes)
            {
                if (childNode.Name == "index")
                {
                    var val = childNode.InnerText;
                    
                    if (val.StartsWith("0x"))
                    {
                        Index = Convert.ToUInt16(val.Substring(2), 16);
                        result = true;
                    }
                }                
            }

            return result;
        }

        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter is Parameter param && param.Index == index && param.SubIndex == subIndex)
                {
                    result = param;
                    break;
                }
            }

            return result;
        }

        public ParameterBase SearchParameter(string uniqueId)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter.UniqueId == uniqueId)
                {
                    result = parameter;
                    break;
                }
            }

            return result;
        }

        #endregion


    }
}
