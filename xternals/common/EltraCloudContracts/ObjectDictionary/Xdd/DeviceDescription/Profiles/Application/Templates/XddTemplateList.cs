using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Templates
{
    public class XddTemplateList
    {
        #region Private fields

        private List<XddFlags> _flags;
        private List<XddAllowedValues> _allowedValueses;
        private readonly XddDataTypeList _dataTypeList;

        #endregion

        #region Constructors

        public XddTemplateList(XddDataTypeList dataTypeList)
        {
            _dataTypeList = dataTypeList;
        }

        #endregion

        #region Properties
        
        public List<XddFlags> Flags => _flags ?? (_flags = new List<XddFlags>()); 

        public List<XddAllowedValues> AllowedValueses => _allowedValueses ?? (_allowedValueses = new List<XddAllowedValues>());

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
                        var flags = new XddFlags(childNode);

                        if (!flags.Parse())
                        {
                            result = false;
                            break;
                        }

                        Flags.Add(flags);
                    }
                    else if (childNode.Name == "allowedValues")
                    {
                        var allowedValues = new XddAllowedValues(_dataTypeList);

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

        public XddFlags FindFlags(string uniqueId)
        {
            XddFlags result = null;

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

        public XddAllowedValues FindAllowedValues(string uniqueIdRef)
        {
            XddAllowedValues result = null;

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
