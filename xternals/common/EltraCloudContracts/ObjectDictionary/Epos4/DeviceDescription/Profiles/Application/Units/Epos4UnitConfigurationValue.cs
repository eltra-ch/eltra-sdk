using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4UnitConfigurationValue
    {
        public Epos4ParamIdRef ParamIdRef { get; set; }

        public Epos4ValueEntry ValueEntry { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "paramIDRef")
                {
                    var paramIdRef = new Epos4ParamIdRef();

                    if (!paramIdRef.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ParamIdRef = paramIdRef;
                }
                else if (childNode.Name == "valueEntry")
                {
                    var valueEntry = new Epos4ValueEntry();

                    if (!valueEntry.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ValueEntry = valueEntry;
                }
            }

            return result;
        }
    }
}
