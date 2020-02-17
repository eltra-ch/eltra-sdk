using System;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes
{
    class Epos4DataType : DataType
    {
        #region Private fields

        private readonly DataTypeList _dataTypeList;
        private readonly XmlNode _source;

        #endregion

        #region Constructors

        public Epos4DataType(XmlNode source, DataTypeList dataTypeList)
        {
            _source = source;

            _dataTypeList = dataTypeList;
        }

        #endregion
        
        #region Methods

        public override bool Parse()
        {
            bool result = false;

            if (_source.Name == "dataType")
            {
                result = ParseDataTypeNode(_source);
            }
            else if (_source.Name == "dataTypeIDRef")
            {
                result = ParseDataTypeRefNode(_source);
            }

            return result;
        }
        private bool ParseDataTypeNode(XmlNode parameterNode)
        {
            bool result = false;
            var typeNode = parameterNode.SelectSingleNode("type");
            var sizeNode = parameterNode.SelectSingleNode("size");

            if (typeNode != null)
            {
                result = ConvertTypeToDataType(typeNode.InnerXml);
            }

            ParseSizeNode(sizeNode);

            return result;
        }

        private void ParseSizeNode(XmlNode sizeNode)
        {
            var unitNode = sizeNode?.Attributes["unit"];
            if (unitNode != null)
            {
                var unit = unitNode.InnerXml;
                switch (unit)
                {
                    case "byte":
                        SizeInBytes = Convert.ToUInt32(sizeNode.InnerXml);
                        break;
                    case "bit":
                        SizeInBits = Convert.ToUInt32(sizeNode.InnerXml);
                        break;
                }
            }
        }
        
        private bool ConvertTypeToDataType(string dataTypeAsString)
        {
            bool result = true;

            switch (dataTypeAsString)
            {
                case "UNSIGNED8":
                    Type = TypeCode.Byte; break;
                case "UNSIGNED16":
                    Type = TypeCode.UInt16; break;
                case "UNSIGNED32":
                    Type = TypeCode.UInt32; break;
                case "UNSIGNED64":
                    Type = TypeCode.UInt64; break;
                case "INTEGER8":
                    Type = TypeCode.SByte; break;
                case "INTEGER16":
                    Type = TypeCode.Int16; break;
                case "INTEGER32":
                    Type = TypeCode.Int32; break;
                case "INTEGER64":
                    Type = TypeCode.Int64; break;
                case "DOUBLE":
                    Type = TypeCode.Double; break;                
                case "VISIBLE_STRING":
                    Type = TypeCode.String; break;
                case "OCTET_STRING":
                    Type = TypeCode.Object; break;
                case "DATE_TIME":
                    Type = TypeCode.DateTime; break;
                case "IDENTITY":
                    Type = TypeCode.Byte; break;
                case "BOOLEAN":
                    Type = TypeCode.Boolean; break;
                default:
                    result = false;
                    break;
            }
            
            return result;
        }
        
        private bool ParseDataTypeRefNode(XmlNode parameterNode)
        {
            bool result = false;
            var uniqueIdRef = parameterNode.Attributes["uniqueIDRef"];

            if (uniqueIdRef != null)
            {
                var reference = _dataTypeList.FindReference(uniqueIdRef.InnerXml);                

                if(reference!=null)
                {
                    Reference = reference;
                    
                    if (reference is Epos4EnumDataTypeReference enumType)
                    {
                        Clone(enumType.DataType);
                    }
                    else if (reference is Epos4StructDataType structType)
                    {
                        Type = TypeCode.Object;

                        SizeInBytes = structType.SizeInBytes;
                        SizeInBits = structType.SizeInBits;
                    }
                    
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}
