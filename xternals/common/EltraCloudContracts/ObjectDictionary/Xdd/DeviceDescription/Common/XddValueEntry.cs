using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common
{
    public class XddValueEntry
    {
        public XddValue Value { get; set; }

        public XddParamIdRef ParamIdRef { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "value")
                {
                    var value = new XddValue();

                    if (!value.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Value = value;
                }
                else if (childNode.Name == "paramIDRef")
                {
                    var paramIdRef = new XddParamIdRef();

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
