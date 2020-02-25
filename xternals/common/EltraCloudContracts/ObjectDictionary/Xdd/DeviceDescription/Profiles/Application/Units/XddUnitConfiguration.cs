using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddUnitConfiguration
    {
        public XddUnitConfigurationValue ConfigurationValue { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node)
            {
                if (childNode.Name == "configurationValue")
                {
                    var configurationValue = new XddUnitConfigurationValue();

                    if (!configurationValue.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ConfigurationValue = configurationValue;
                }
            }

            return result;
        }

        public string GetConfigurationValue()
        {
            string result = string.Empty;

            var valueEntry = ConfigurationValue?.ValueEntry;

            var valueEntryValue = valueEntry?.Value;

            if (valueEntryValue != null)
            {
                result = valueEntryValue.Value;
            }

            return result;
        }
    }
}
