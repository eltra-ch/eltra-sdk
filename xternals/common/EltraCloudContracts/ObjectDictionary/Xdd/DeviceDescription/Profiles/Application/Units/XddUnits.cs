using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddUnits
    {
        private List<XddUnitPhysicalQuantity> _unitPhysicalQuantityList;
        private readonly XddParameterList _parameterList;

        public XddUnits(XddParameterList parameterList)
        {
            _parameterList = parameterList;
        }

        public string UniqueId { get; set; }

        public string KindOfAccess { get; set; }

        public List<XddUnitPhysicalQuantity> UnitPhysicalQuantityList
        {
            get => _unitPhysicalQuantityList ?? (_unitPhysicalQuantityList = new List<XddUnitPhysicalQuantity>());
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
                    var unitPhysicalQuantity = new XddUnitPhysicalQuantity(_parameterList);

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

        public XddUnit FindPhysicalQuantityIdRef(string uniqueIdRef)
        {
            XddUnit result = null;

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
