using System.Collections.Generic;
using System.Xml;
using EltraCommon.Logger;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4UnitPhysicalQuantity
    {
        private List<Epos4Label> _labels;
        private List<Epos4Unit> _units;
        private readonly ParameterList _parameterList;

        public Epos4UnitPhysicalQuantity(ParameterList parameterList)
        {
            _parameterList = parameterList;
        }

        public string UniqueId { get; set; }

        public string Type { get; set; }

        public List<Epos4Label> Labels => _labels ?? (_labels = new List<Epos4Label>());

        public List<Epos4Unit> Units => _units ?? (_units = new List<Epos4Unit>());

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;
            Type = node.Attributes["type"].InnerXml;
            
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
                else if (childNode.Name == "unit")
                {
                    var unit = new Epos4Unit();

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

        public Epos4Unit GetActiveUnit()
        {
            Epos4Unit result = null;

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
