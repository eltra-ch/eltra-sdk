using System;
using System.Collections.Generic;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes
{
    class XddEnumDataTypeReference : XddDataTypeReference
    {
        #region Private fields

        private List<XddEnumEntry> _enumEntries;
        private readonly XddDataTypeList _dataTypeList;
        private readonly XmlNode _source;

        public List<XddEnumEntry> EnumEntries => _enumEntries ?? (_enumEntries = new List<XddEnumEntry>());

        #endregion

        #region Constructors

        public XddEnumDataTypeReference(XmlNode source, XddDataTypeList dataTypeList)
        {
            _source = source;
            _dataTypeList = dataTypeList;
        }

        #endregion

        public DataType DataType { get; set; }

        #region Methods

        public bool GetValueLabel<T>(T value, out string enumValueLabel)
        {
            bool result = false;
            const string langCode = "en-TT";

            enumValueLabel = string.Empty;

            foreach (var enumEntry in EnumEntries)
            {
                long longValue = 0;

                if (typeof(T) == typeof(sbyte))
                {
                    longValue = (sbyte)(object)value;
                }
                else if (typeof(T) == typeof(byte))
                {
                    longValue = (byte)(object)value;
                }
                else if (typeof(T) == typeof(short))
                {
                    longValue = (short)(object)value;
                }
                else if (typeof(T) == typeof(ushort))
                {
                    longValue = (ushort)(object)value;
                }
                else if (typeof(T) == typeof(int))
                {
                    longValue = (int)(object)value;
                }
                else if (typeof(T) == typeof(uint))
                {
                    longValue = (uint)(object)value;
                }
                else if (typeof(T) == typeof(long))
                {
                    longValue = (long)(object)value;
                }

                if (enumEntry.Value == longValue)
                {
                    enumValueLabel = enumEntry.GetLabel(langCode);
                    result = true;
                    break;
                }
            }

            return result;
        }

        public override bool Parse()
        {
            bool result = false;

            if (_source.Attributes != null)
            {
                UniqueId = _source.Attributes["uniqueID"].InnerXml;
                Name = _source.Attributes["name"].InnerXml;
            }

            foreach (XmlNode childNode in _source)
            {
                if (childNode.Name == "dataType")
                {
                    DataType = new XddDataType(childNode, _dataTypeList);

                    result = DataType.Parse();
                }
                else if (childNode.Name == "enumEntry")
                {
                    var enumEntry = new XddEnumEntry(childNode);

                    result = enumEntry.Parse();

                    EnumEntries.Add(enumEntry);
                }
            }

            return result;
        }

        #endregion
    }
}
