using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common
{
    class XddDescription : Description
    {
        private readonly XmlNode _source;

        public XddDescription(XmlNode source)
        {
            _source = source;
        }

        public override bool Parse()
        {
            var langAttribute = _source.Attributes["lang"];

            Lang = langAttribute?.InnerXml;
            Content = _source.InnerXml;

            return true;
        }
    }
}
