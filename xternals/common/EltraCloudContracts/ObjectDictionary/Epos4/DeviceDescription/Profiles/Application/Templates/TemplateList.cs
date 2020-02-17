using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Templates
{
    public class TemplateList
    {
        #region Private fields

        private List<Epos4Flags> _flags;
        private List<AllowedValues> _allowedValueses;
        private readonly DataTypeList _dataTypeList;

        #endregion

        #region Constructors

        public TemplateList(DataTypeList dataTypeList)
        {
            _dataTypeList = dataTypeList;
        }

        #endregion

        #region Properties
        
        public List<Epos4Flags> Flags => _flags ?? (_flags = new List<Epos4Flags>()); 

        public List<AllowedValues> AllowedValueses => _allowedValueses ?? (_allowedValueses = new List<AllowedValues>());

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            try
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "flags")
                    {
                        var flags = new Epos4Flags(childNode);

                        if (!flags.Parse())
                        {
                            result = false;
                            break;
                        }

                        Flags.Add(flags);
                    }
                    else if (childNode.Name == "allowedValues")
                    {
                        var allowedValues = new AllowedValues(_dataTypeList);

                        if (!allowedValues.Parse(childNode))
                        {
                            result = false;
                            break;
                        }

                        AllowedValueses.Add(allowedValues);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            return result;
        }

        public Epos4Flags FindFlags(string uniqueId)
        {
            Epos4Flags result = null;

            foreach (var flags in Flags)
            {
                if (flags.UniqueId == uniqueId)
                {
                    result = flags;
                    break;
                }
            }

            return result;
        }

        public AllowedValues FindAllowedValues(string uniqueIdRef)
        {
            AllowedValues result = null;

            foreach (var allowedValue in AllowedValueses)
            {
                if (allowedValue.UniqueId == uniqueIdRef)
                {
                    result = allowedValue;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
