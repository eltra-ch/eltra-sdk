using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common
{
    class Epos4Description : Description
    {
        private readonly XmlNode _source;

        public Epos4Description(XmlNode source)
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
