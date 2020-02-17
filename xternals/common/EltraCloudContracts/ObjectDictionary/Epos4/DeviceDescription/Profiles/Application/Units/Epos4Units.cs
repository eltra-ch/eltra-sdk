using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4Units
    {
        private List<Epos4UnitPhysicalQuantity> _unitPhysicalQuantityList;
        private readonly ParameterList _parameterList;

        public Epos4Units(ParameterList parameterList)
        {
            _parameterList = parameterList;
        }

        public string UniqueId { get; set; }

        public string KindOfAccess { get; set; }

        public List<Epos4UnitPhysicalQuantity> UnitPhysicalQuantityList
        {
            get => _unitPhysicalQuantityList ?? (_unitPhysicalQuantityList = new List<Epos4UnitPhysicalQuantity>());
        }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            UniqueId = node.Attributes["uniqueID"].InnerXml;
            KindOfAccess = node.Attributes["kindOfAccess"].InnerXml;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "unitPhysicalQuantity")
                {
                    var unitPhysicalQuantity = new Epos4UnitPhysicalQuantity(_parameterList);

                    if (!unitPhysicalQuantity.Parse(childNode))
                    {
                        result = false;
                    }
                    else
                    {
                        UnitPhysicalQuantityList.Add(unitPhysicalQuantity);
                    }
                }

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        public Epos4Unit FindPhysicalQuantityIdRef(string uniqueIdRef)
        {
            Epos4Unit result = null;

            foreach (var unitPhysicalQuantity in UnitPhysicalQuantityList)
            {
                if (unitPhysicalQuantity.UniqueId == uniqueIdRef)
                {
                    result = unitPhysicalQuantity.GetActiveUnit();

                    break;
                }
            }

            return result;
        }
    }
}
