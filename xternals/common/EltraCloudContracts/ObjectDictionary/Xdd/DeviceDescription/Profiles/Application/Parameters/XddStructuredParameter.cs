using System;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    class XddStructuredParameter : StructuredParameter
    {
        public XddStructuredParameter(Contracts.Devices.EltraDevice device, XmlNode source)
            :base(device, source)
        {   
        }
        
        #region Methods

        public override bool Parse()
        {
            bool result = base.Parse();

            if (result)
            {
                foreach (XmlNode childNode in Source.ChildNodes)
                {
                    if (childNode.Name == "label")
                    {
                        var label = new XddLabel(childNode);

                        if (label.Parse())
                        {
                            Labels.Add(label);
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
