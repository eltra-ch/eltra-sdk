using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common
{
    public class Epos4ValueEntry
    {
        public Epos4Value Value { get; set; }

        public Epos4ParamIdRef ParamIdRef { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "value")
                {
                    var value = new Epos4Value();

                    if (!value.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Value = value;
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
            }

            return result;
        }
    }
}
