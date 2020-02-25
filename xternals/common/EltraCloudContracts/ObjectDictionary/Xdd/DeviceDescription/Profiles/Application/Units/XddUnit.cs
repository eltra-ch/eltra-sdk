using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddUnit : Unit
    {
        #region Private fields

        private List<XddLabel> _labels;
        private XddDecimalPlaces _epos4DecimalPlaces;
        private XddMultiplier _epos4Multiplier;

        #endregion

        #region Properties

        private List<XddLabel> Labels => _labels ?? (_labels = new List<XddLabel>());

        private XddMultiplier Epos4Multiplier
        {
            get => _epos4Multiplier ?? (_epos4Multiplier = new XddMultiplier());
            set => _epos4Multiplier = value;
        }

        public XddDecimalPlaces Epos4DecimalPlaces
        {
            get => _epos4DecimalPlaces ?? (_epos4DecimalPlaces = new XddDecimalPlaces());
            set => _epos4DecimalPlaces = value;
        }

        public XddUnitConfiguration Configuration { get; set; }

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
                    var label = new XddLabel(childNode);

                    if (!label.Parse())
                    {
                        result = false;
                        break;
                    }

                    Labels.Add(label);
                }
                else if (childNode.Name == "multiplier")
                {
                    var epos4Multiplier = new XddMultiplier();

                    if (!epos4Multiplier.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Epos4Multiplier = epos4Multiplier;
                }
                else if(childNode.Name == "configuration")
                {
                    Configuration = new XddUnitConfiguration();

                    if (!Configuration.Parse(childNode))
                    {
                        result = false;
                    }
                }
                else if(childNode.Name == "decimalPlaces")
                {
                    var epos4DecimalPlaces = new XddDecimalPlaces();

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
