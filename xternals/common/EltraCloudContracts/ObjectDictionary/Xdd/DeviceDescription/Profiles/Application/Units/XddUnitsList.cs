using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units
{
    public class XddUnitsList
    {
        private List<XddUnits> _units;
        private readonly XddParameterList _parameterList;

        public XddUnitsList(XddParameterList parameterList)
        {
            _parameterList = parameterList;
        }
        
        public List<XddUnits> Units => _units ?? (_units = new List<XddUnits>());

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "units")
                {
                    var units = new XddUnits(_parameterList);

                    if (!units.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    Units.Add(units);
                }
            }

            return result;
        }

        public XddUnit FindUnitReference(string uniqueIdRef)
        {
            XddUnit result = null;

            foreach (var units in Units)
            {
                var unit = units.FindPhysicalQuantityIdRef(uniqueIdRef);

                if (unit != null)
                {
                    result = unit;
                    break;
                }
            }

            return result;
        }
    }
}
