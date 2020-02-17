using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes
{
    public class DataTypeList
    {
        #region Private fields

        private Dictionary<string, Epos4DataTypeReference> _dataTypes;
        
        private Dictionary<string, Epos4DataTypeReference> DataTypes => _dataTypes ?? (_dataTypes = new Dictionary<string, Epos4DataTypeReference>());

        #endregion

        #region Methods

        public bool Parse(XmlNode dataTypeListNode)
        {
            bool result = true;

            try
            {
                foreach (XmlNode childNode in dataTypeListNode.ChildNodes)
                {
                    Epos4DataTypeReference dataType = null;

                    if (childNode.Name == "enum")
                    {
                        dataType = new Epos4EnumDataTypeReference(childNode, this);
                    }
                    else if (childNode.Name == "struct")
                    {
                        dataType = new Epos4StructDataType(childNode, this);
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

        internal Epos4DataTypeReference FindReference(string uniqueId)
        {
            Epos4DataTypeReference result = null;

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
                if (dataTypeReference is Epos4EnumDataTypeReference enumDataType)
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

        internal Epos4EnumEntry FindReferenceEnumEntry(string uniqueId)
        {
            Epos4EnumEntry result = null;

            foreach (var dataTypeKeyValuePair in DataTypes)
            {
                var dataTypeReference = dataTypeKeyValuePair.Value;
                if (dataTypeReference is Epos4EnumDataTypeReference enumDataType)
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
                if (dataTypeReference is Epos4EnumDataTypeReference enumDataType)
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
