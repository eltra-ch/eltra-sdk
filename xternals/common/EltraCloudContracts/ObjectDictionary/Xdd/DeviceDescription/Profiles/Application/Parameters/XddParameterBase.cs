using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    public class XddParameterBase : Parameter
    {
        public XddParameterBase(Contracts.Devices.EltraDevice device, XmlNode source)
            : base(device, source)
        {            
        }

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
    }

}
