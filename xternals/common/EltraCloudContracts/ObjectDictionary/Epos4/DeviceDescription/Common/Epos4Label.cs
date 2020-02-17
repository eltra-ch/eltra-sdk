using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common
{
    public class Epos4Label : Label
    {
        private XmlNode Source { get; set; }

        public Epos4Label(XmlNode source)
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
