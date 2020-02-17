using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units
{
    public class Epos4UnitsList
    {
        private List<Epos4Units> _units;
        private readonly ParameterList _parameterList;

        public Epos4UnitsList(ParameterList parameterList)
        {
            _parameterList = parameterList;
        }
        
        public List<Epos4Units> Units => _units ?? (_units = new List<Epos4Units>());

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "units")
                {
                    var units = new Epos4Units(_parameterList);

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

        public Epos4Unit FindUnitReference(string uniqueIdRef)
        {
            Epos4Unit result = null;

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
