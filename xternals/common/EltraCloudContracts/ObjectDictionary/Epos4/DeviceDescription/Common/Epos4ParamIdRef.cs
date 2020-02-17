using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common
{
    public class Epos4ParamIdRef
    {
        public string UniqueIdRef { get; set; }
        
        public bool Parse(XmlNode childNode)
        {
            bool result = false;
            var uniqueIdAttribute = childNode.Attributes?["uniqueIDRef"];

            if (uniqueIdAttribute != null)
            {
                UniqueIdRef = uniqueIdAttribute.InnerXml;

                result = true;
            }

            return result;
        }
    }
}
