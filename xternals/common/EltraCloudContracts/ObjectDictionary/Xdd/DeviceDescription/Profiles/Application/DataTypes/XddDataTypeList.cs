using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes
{
    public class XddDataTypeList
    {
        #region Private fields

        private Dictionary<string, XddDataTypeReference> _dataTypes;
        
        private Dictionary<string, XddDataTypeReference> DataTypes => _dataTypes ?? (_dataTypes = new Dictionary<string, XddDataTypeReference>());

        #endregion

        #region Methods

        public bool Parse(XmlNode dataTypeListNode)
        {
            bool result = true;

            try
            {
                foreach (XmlNode childNode in dataTypeListNode.ChildNodes)
                {
                    XddDataTypeReference dataType = null;

                    if (childNode.Name == "enum")
                    {
                        dataType = new XddEnumDataTypeReference(childNode, this);
                    }
                    else if (childNode.Name == "struct")
                    {
                        dataType = new XddStructDataType(childNode, this);
                    }

                    if (dataType != null && dataType.Parse())
                    {
                        DataTypes.Add(dataType.UniqueId, dataType);
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return result;
        }

        internal XddDataTypeReference FindReference(string uniqueId)
        {
            XddDataTypeReference result = null;

            if (DataTypes.ContainsKey(uniqueId))
            {
                result = DataTypes[uniqueId];
            }

            return result;
        }

        internal string FindReferenceValue(string uniqueId)
        {
            string result = string.Empty;
            
            foreach (var dataTypeKeyValuePair in DataTypes)
            {
                var dataTypeReference = dataTypeKeyValuePair.Value;
                if (dataTypeReference is XddEnumDataTypeReference enumDataType)
                {
                    foreach (var enumEntry in enumDataType.EnumEntries)
                    {
                        if (enumEntry.UniqueId == uniqueId)
                        {
                            result = $"{enumEntry.Value}";
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            
            return result;
        }

        internal XddEnumEntry FindReferenceEnumEntry(string uniqueId)
        {
            XddEnumEntry result = null;

            foreach (var dataTypeKeyValuePair in DataTypes)
            {
                var dataTypeReference = dataTypeKeyValuePair.Value;
                if (dataTypeReference is XddEnumDataTypeReference enumDataType)
                {
                    foreach (var enumEntry in enumDataType.EnumEntries)
                    {
                        if (enumEntry.UniqueId == uniqueId)
                        {
                            result = enumEntry;
                            break;
                        }
                    }
                }

                if (result!=null)
                {
                    break;
                }
            }

            return result;
        }

        internal DataType FindReferencedDataType(string uniqueId)
        {
            DataType result = null;

            foreach (var dataTypeKeyValuePair in DataTypes)
            {
                var dataTypeReference = dataTypeKeyValuePair.Value;
                if (dataTypeReference is XddEnumDataTypeReference enumDataType)
                {
                    foreach (var enumEntry in enumDataType.EnumEntries)
                    {
                        if (enumEntry.UniqueId == uniqueId)
                        {
                            result = enumDataType.DataType;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
