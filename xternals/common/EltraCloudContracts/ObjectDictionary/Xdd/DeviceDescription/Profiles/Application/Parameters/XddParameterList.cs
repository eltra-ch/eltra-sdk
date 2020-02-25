using System;
using System.Collections.Generic;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Templates;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    public class XddParameterList
    {
        #region Private fields

        private List<ParameterBase> _parameters;
        private readonly ParameterComparer _parameterComparer;

        private readonly XddDataTypeList _dataTypeList;
        private readonly XddTemplateList _templateList;
        private XddUnitsList _unitsList;

        private readonly XddDeviceManager _deviceManager;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public XddParameterList(Contracts.Devices.EltraDevice device, XddDeviceManager deviceManager, XddDataTypeList dataTypeList, XddTemplateList templateList)
        {
            _parameterComparer = new ParameterComparer();

            _device = device;
            _deviceManager = deviceManager;

            _dataTypeList = dataTypeList;
            _templateList = templateList;
        }

        #endregion

        #region Properties
        
        public List<ParameterBase> Parameters => _parameters ?? (_parameters = new List<ParameterBase>());

        public XddUnitsList UnitsList
        {
            get => _unitsList;
            set => _unitsList = value;
        }

        #endregion

        #region Methods

        public bool Parse(XmlNode profileNode)
        {
            bool result = false;

            try
            {
                foreach (XmlNode childNode in profileNode.ChildNodes)
                {
                    if(childNode.Name == "#comment")
                    {
                        result = true;
                    }
                    else if (childNode.Name == "parameter")
                    {
                        var parameter = new XddParameter(_device, childNode, _deviceManager, _dataTypeList, _templateList);

                        if (parameter.Parse())
                        {
                            var structuredParameter = FindStructuredParameter(parameter.Index);

                            result = structuredParameter?.AddParameter(parameter) ?? AddParameter(parameter);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else if (childNode.Name == "structuredParameter")
                    {
                        var structuredParameter = new XddStructuredParameter(_device, childNode);

                        if (structuredParameter.Parse())
                        {
                            AddParameter(structuredParameter);
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }

                    if (!result)
                    {
                        break;
                    }
                }

                Parameters.Sort(_parameterComparer);

                ResolveUnitsReferences(Parameters);
                ResolveAllowedValuesReferences(Parameters);
                ResolveDefaultValuesReferences(Parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        private void ResolveUnitsReferences(List<ParameterBase> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is XddParameter epos4Parameter)
                {
                    if(epos4Parameter.Unit is XddUnitReference unitReference)
                    {
                        epos4Parameter.Unit = _unitsList.FindUnitReference(unitReference.UniqueId);
                    }
                }

                ResolveUnitsReferences(parameter.Parameters);
            }
        }

        private void ResolveAllowedValuesReferences(List<ParameterBase> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is XddParameter epos4Parameter)
                {
                    foreach(var allowedValues in epos4Parameter.AllowedValues)
                    {
                        var range = allowedValues.Range;

                        if (range != null)
                        {
                            foreach (var minValue in range.MinValues)
                            {
                                var paramIdRef = minValue.ParamIdRef;
                                if (paramIdRef != null)
                                {
                                    if (FindParameter(paramIdRef.UniqueIdRef) is XddParameter refParam)
                                    {
                                        minValue.Value.Value = refParam.ActualValue.Value;
                                    }
                                }
                            }

                            foreach (var maxValue in range.MaxValues)
                            {
                                var paramIdRef = maxValue.ParamIdRef;
                                if (paramIdRef != null)
                                {
                                    if (FindParameter(paramIdRef.UniqueIdRef) is XddParameter refParam)
                                    {
                                        if (refParam.ActualValue.IsValid)
                                        {
                                            maxValue.Value = new XddValue(refParam.ActualValue, refParam.DataType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ResolveAllowedValuesReferences(parameter.Parameters);
            }
        }

        private void ResolveDefaultValuesReferences(List<ParameterBase> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is XddParameter epos4Parameter)
                {
                    if(epos4Parameter.DefaultValue is XddDefaultValue epos4DefaultValue)
                    {
                        var paramIdRef = epos4DefaultValue.ParamIdRef;

                        if (paramIdRef != null)
                        {
                            if (FindParameter(paramIdRef.UniqueIdRef) is XddParameter refParam)
                            {
                                epos4DefaultValue.Value = refParam.DefaultValue.Value;
                            }
                        }
                    }                    
                }

                ResolveDefaultValuesReferences(parameter.Parameters);
            }
        }

        private bool AddParameter(ParameterBase parameter)
        {
            bool result = false;

            if (parameter != null)
            {
                Parameters.Add(parameter);

                result = true;
            }

            return result;
        }

        private ParameterBase FindStructuredParameter(ushort index)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter is StructuredParameter structuredParameter)
                {
                    if (structuredParameter.Index == index)
                    {
                        result = structuredParameter;
                        break;
                    }
                }
            }

            return result;
        }

        public ParameterBase FindParameter(string uniqueIdRef)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter is StructuredParameter structuredParameter)
                {
                    if (structuredParameter.UniqueId == uniqueIdRef)
                    {
                        result = structuredParameter;
                        break;
                    }

                    foreach (var structuredSubParameter in structuredParameter.Parameters)
                    {
                        if (structuredSubParameter.UniqueId == uniqueIdRef)
                        {
                            result = structuredSubParameter;
                            break;
                        }
                    }

                    if (result != null)
                    {
                        break;
                    }
                }
                else if (parameter is Parameter parameterEntry)
                {
                    if (parameterEntry.UniqueId == uniqueIdRef)
                    {
                        result = parameterEntry;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion


    }
}
