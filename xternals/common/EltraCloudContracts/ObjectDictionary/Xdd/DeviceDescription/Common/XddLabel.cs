using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common
{
    public class XddLabel : Label
    {
        private XmlNode Source { get; set; }

        public XddLabel(XmlNode source)
        {
            Source = source;
        }

        public override bool Parse()
        {
            var langAttribute = Source.Attributes["lang"];

            Lang = langAttribute?.InnerXml;
            Content = Source.InnerXml;

            return true;
        }

    }
}
