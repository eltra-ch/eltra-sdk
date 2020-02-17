using System.Globalization;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4Multiplier
    {
        public Epos4Multiplier()
        {
            Value = 1;
        }

        public double Value { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = false;

            if (double.TryParse(node.InnerXml, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                Value = value;
                result = true;
            }

            return result;
        }
    }
}
