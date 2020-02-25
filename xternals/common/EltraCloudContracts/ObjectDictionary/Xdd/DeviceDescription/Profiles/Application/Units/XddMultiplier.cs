using System.Globalization;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddMultiplier
    {
        public XddMultiplier()
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
