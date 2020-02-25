using System;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes;
using EltraCommon.Logger;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    class XddDefaultValue : ParameterValue
    {
        private readonly XmlNode _source;
        private readonly XddDataTypeList _dataTypeList;
        private readonly DataType _dataType;

        public XddDefaultValue(XmlNode source, DataType dataType, XddDataTypeList dataTypeList)
        {
            _source = source;
            _dataType = dataType;
            _dataTypeList = dataTypeList;
        }

        public XddParamIdRef ParamIdRef { get; set; }


        public bool Parse()
        {
            bool result = false;

            try
            {
                if (_source.ChildNodes.Count > 0)
                {
                    foreach (XmlNode childNode in _source.ChildNodes)
                    {
                        if (childNode is XmlText)
                        {
                            result = SetDefaultValue(_dataType, _source.InnerXml.Trim());
                        }
                        else if (childNode.Name == "enumEntryIDRef")
                        {
                            var uniqueIdRef = childNode.Attributes["uniqueIDRef"].InnerXml;
                            var value = _dataTypeList.FindReferenceValue(uniqueIdRef);
                            var dataType = _dataTypeList.FindReferencedDataType(uniqueIdRef);

                            if (value != null)
                            {
                                result = SetDefaultValue(dataType, value);
                            }

                            break;
                        }
                        else if (childNode.Name == "paramIDRef")
                        {
                            var paramIdRef = new XddParamIdRef();

                            if (!paramIdRef.Parse(childNode))
                            {
                                result = false;
                                break;
                            }

                            ParamIdRef = paramIdRef;

                            result = true;

                            break;
                        }
                        else if (childNode.Name == "value")
                        {
                            result = SetDefaultValue(_dataType, childNode.InnerXml.Trim());
                        }
                    }
                }
                else
                {
                    result = SetDefaultValue(_dataType, _source.InnerXml.Trim());
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception("Epos4DefaultValue - Parse", e);
            }

            return result;
        }
    }
}
