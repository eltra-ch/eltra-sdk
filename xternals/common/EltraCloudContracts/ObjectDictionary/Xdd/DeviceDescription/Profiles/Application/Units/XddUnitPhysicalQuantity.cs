using System.Collections.Generic;
using System.Xml;
using EltraCommon.Logger;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddUnitPhysicalQuantity
    {
        private List<XddLabel> _labels;
        private List<XddUnit> _units;
        private readonly XddParameterList _parameterList;

        public XddUnitPhysicalQuantity(XddParameterList parameterList)
        {
            _parameterList = parameterList;
        }

        public string UniqueId { get; set; }

        public string Type { get; set; }

        public List<XddLabel> Labels => _labels ?? (_labels = new List<XddLabel>());

        public List<XddUnit> Units => _units ?? (_units = new List<XddUnit>());

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;
            Type = node.Attributes["type"].InnerXml;
            
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
                else if (childNode.Name == "unit")
                {
                    var unit = new XddUnit();

                    if (!unit.Parse(childNode))
                    {
                        result = false;
                    }

                    Units.Add(unit);
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        public XddUnit GetActiveUnit()
        {
            XddUnit result = null;

            if (Units.Count == 1)
            {
                result = Units[0];
            }
            else
            {
                foreach (var unit in Units)
                {
                    string uniqueIdRef = unit.GetConfigurationParameterIdRef();

                    var parameter = _parameterList.FindParameter(uniqueIdRef) as Parameter;

                    if (unit.HasMatchingValue(parameter))
                    {
                        result = unit;
                        break;
                    }
                    else
                    {
                        MsgLogger.WriteError("Epos4UnitPhysicalQuantity - GetActiveUnit", $"GetActiveUnit - Parameter '{uniqueIdRef}' not found!");
                    }
                }
            }

            return result;
        }
    }
}
