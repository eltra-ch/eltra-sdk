using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4Unit : Unit
    {
        #region Private fields

        private List<Epos4Label> _labels;
        private Epos4DecimalPlaces _epos4DecimalPlaces;
        private Epos4Multiplier _epos4Multiplier;

        #endregion

        #region Properties

        private List<Epos4Label> Labels => _labels ?? (_labels = new List<Epos4Label>());

        private Epos4Multiplier Epos4Multiplier
        {
            get => _epos4Multiplier ?? (_epos4Multiplier = new Epos4Multiplier());
            set => _epos4Multiplier = value;
        }

        public Epos4DecimalPlaces Epos4DecimalPlaces
        {
            get => _epos4DecimalPlaces ?? (_epos4DecimalPlaces = new Epos4DecimalPlaces());
            set => _epos4DecimalPlaces = value;
        }

        public Epos4UnitConfiguration Configuration { get; set; }

        #endregion

        #region Methods

        protected override double GetMultiplier()
        {
            return Epos4Multiplier.Value;
        }

        protected override int GetDecimalPlaces()
        {
            return Epos4DecimalPlaces.Value;
        }

        public bool Parse(XmlNode node)
        {
            bool result = true;
            var uniqueIdAttribute = node.Attributes["uniqueID"];

            if(uniqueIdAttribute!=null)
            {
                UniqueId = uniqueIdAttribute.InnerXml;
            }

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "label")
                {
                    var label = new Epos4Label(childNode);

                    if (!label.Parse())
                    {
                        result = false;
                        break;
                    }

                    Labels.Add(label);
                }
                else if (childNode.Name == "multiplier")
                {
                    var epos4Multiplier = new Epos4Multiplier();

                    if (!epos4Multiplier.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Epos4Multiplier = epos4Multiplier;
                }
                else if(childNode.Name == "configuration")
                {
                    Configuration = new Epos4UnitConfiguration();

                    if (!Configuration.Parse(childNode))
                    {
                        result = false;
                    }
                }
                else if(childNode.Name == "decimalPlaces")
                {
                    var epos4DecimalPlaces = new Epos4DecimalPlaces();

                    if (!epos4DecimalPlaces.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Epos4DecimalPlaces = epos4DecimalPlaces;
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        protected override string GetLabel(string lang)
        {
            string result = string.Empty;

            foreach (var label in Labels)
            {
                if (label.Lang == lang)
                {
                    result = label.Content;
                    break;
                }
            }

            return result;
        }

        public string GetConfigurationParameterIdRef()
        {
            return Configuration.ConfigurationValue.ParamIdRef.UniqueIdRef;
        }

        public bool HasMatchingValue(Parameter parameter)
        {
            bool result = false;

            string configurationValue = Configuration.GetConfigurationValue();
            var actualValue = parameter?.ActualValue;

            if (actualValue != null)
            {
                if (configurationValue == actualValue.Value)
                {
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}
