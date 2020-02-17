using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4UnitConfiguration
    {
        public Epos4UnitConfigurationValue ConfigurationValue { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node)
            {
                if (childNode.Name == "configurationValue")
                {
                    var configurationValue = new Epos4UnitConfigurationValue();

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
