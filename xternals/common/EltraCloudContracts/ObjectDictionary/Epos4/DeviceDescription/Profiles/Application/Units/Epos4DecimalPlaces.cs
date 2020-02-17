using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4DecimalPlaces
    {
        public Epos4DecimalPlaces()
        {
            Value = 0;
        }

        public int Value { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = false;

            if (int.TryParse(node.InnerXml, out var value))
            {
                Value = value;
                result = true;
            }

            return result;
        }
    }
}
