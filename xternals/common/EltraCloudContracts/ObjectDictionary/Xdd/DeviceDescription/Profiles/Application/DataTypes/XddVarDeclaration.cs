using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes
{
    class XddVarDeclaration
    {
        #region Private fields
        
        private List<Label> _labels;
        private readonly XddDataTypeList _dataTypeList;

        #endregion

        #region Constructors
        
        public XddVarDeclaration(XddDataTypeList dataTypeList)
        {
            _dataTypeList = dataTypeList;
        }

        #endregion

        #region Properties

        public string UniqueId { get; set; }

        public string Name { get; set; }

        public XddDataType DataType { get; private set; }

        public List<Label> Labels { get => _labels ?? (_labels = new List<Label>()); }

        public string DisplayFormat { get; set; }

        #endregion

        #region Methods

        private bool GetReferenceLabel<T>(T value, out string refLabel)
        {
            bool result = false;

            refLabel = string.Empty;

            if (DataType.Reference != null)
            {
                if (DataType.Reference is XddEnumDataTypeReference enumReference)
                {
                    if (enumReference.GetValueLabel(value, out var label))
                    {
                        refLabel = label;
                        result = true;
                    }
                }
            }

            return result;
        }

        public bool GetValue<T>(byte[] byteArray, out T value)
        {
            bool result = false;

            value = default;

            if (byteArray!= null && byteArray.Length > 0)
            {
                switch (DataType.Type)
                {
                        case TypeCode.Boolean:
                        {
                            value = (T)(object)(byteArray[0] > 0);
                            result = true;
                        }
                        break;
                        case TypeCode.SByte:
                        {
                            sbyte[] signed = Array.ConvertAll(byteArray, b => unchecked((sbyte)b));

                            if (typeof(T) == typeof(string) && GetReferenceLabel(signed[0], out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)signed[0];
                            }

                            result = true;
                        }
                        break;
                        case TypeCode.Byte:
                        {
                            if (typeof(T) == typeof(string) && GetReferenceLabel(byteArray[0], out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)byteArray[0];
                            }

                            result = true;
                        }
                        break;
                        case TypeCode.Char:
                        {
                            value = (T)(object)BitConverter.ToChar(byteArray,0);
                            result = true;
                        }
                        break;
                        case TypeCode.Int16:
                        {
                            if (GetReferenceLabel(BitConverter.ToInt16(byteArray, 0), out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)BitConverter.ToInt16(byteArray, 0);
                            }

                            result = true;
                        }
                        break;
                        case TypeCode.Int32:
                        {
                            if (typeof(T) == typeof(string) && GetReferenceLabel(BitConverter.ToInt32(byteArray, 0), out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)BitConverter.ToInt32(byteArray, 0);
                            }
                                
                            result = true;
                        }
                        break;
                        case TypeCode.Int64:
                        {
                            value = (T)(object)BitConverter.ToInt64(byteArray,0);
                            result = true;
                        }
                        break;
                        case TypeCode.UInt16:
                        {
                            if (typeof(T) == typeof(string) && GetReferenceLabel(BitConverter.ToUInt16(byteArray, 0), out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)BitConverter.ToUInt16(byteArray, 0);
                            }

                            result = true;
                        }
                        break;
                        case TypeCode.UInt32:
                        {
                            if (typeof(T) == typeof(string) && GetReferenceLabel(BitConverter.ToUInt32(byteArray, 0), out string label))
                            {
                                value = (T)(object)label;
                            }
                            else
                            {
                                value = (T)(object)BitConverter.ToUInt32(byteArray, 0);
                            }
                                
                            result = true;
                        }
                        break;
                        case TypeCode.UInt64:
                        {
                            value = (T)(object)BitConverter.ToUInt64(byteArray,0);
                            result = true;
                        }
                        break;
                        case TypeCode.Object:
                        {
                            value = (T)(object)byteArray;
                            result = true;
                        }
                        break;
                        case TypeCode.String:
                        {
                            value = (T)(object)Encoding.Unicode.GetString(byteArray);
                            result = true;
                        }
                        break;
                        case TypeCode.Double:
                        {
                            value = (T)(object)BitConverter.ToDouble(byteArray,0);
                            result = true;
                        }
                        break;
                        case TypeCode.DateTime:
                        {
                            long dateData = BitConverter.ToInt64(byteArray,0);

                            var val = DateTime.FromBinary(dateData);

                            value = (T)(object)val;

                            result = true;
                        }
                        break;
                }
            }
            
            return result;
        }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            if (node.Attributes != null)
            {
                UniqueId = node.Attributes["uniqueID"].InnerXml;
                Name = node.Attributes["name"].InnerXml;
            }

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "dataType" || childNode.Name == "dataTypeIDRef")
                {
                    DataType = new XddDataType(childNode, _dataTypeList);

                    if (!DataType.Parse())
                    {
                        result = false;
                        break;
                    }
                }
                else if (childNode.Name == "label")
                {
                    var label = new XddLabel(childNode);

                    if (!label.Parse())
                    {
                        result = false;
                        break;
                    }

                    Labels.Add(label);
                }
                else if (childNode.Name == "displayFormat")
                {
                    DisplayFormat = childNode.InnerXml.Trim();
                }
            }

            return result;
        }

        #endregion

        
    }
}
