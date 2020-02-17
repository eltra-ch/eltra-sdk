using System;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Templates;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.UserLevels;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters
{
    public class Epos4Parameter : Epos4ParameterBase
    {
        #region Private fields

        private UserLevelList _userLevels;

        private readonly TemplateList _templateList;
        private readonly DataTypeList _dataTypeList;        
        private readonly DeviceManager _deviceManager;
                        
        #endregion

        #region Constructors

        public Epos4Parameter(EltraDevice device, XmlNode source, DeviceManager deviceManager, DataTypeList dataTypeList, TemplateList templateList)
            : base(device, source)
        {
            _deviceManager = deviceManager;
            _templateList = templateList;
            _dataTypeList = dataTypeList;

            Unit = new Epos4Unit();
        }

        #endregion

        #region Properties
        
        public UserLevelList UserLevels { get => _userLevels ?? (_userLevels = new UserLevelList(_deviceManager)); }

        #endregion

        #region Methods

        public bool VisibleBy(string role)
        {
            return UserLevels.HasRole(role);
        }

        public bool VisibleByAny()
        {
            return UserLevels.IsRoleUndefined;
        }

        public override bool Parse()
        {
            bool result = base.Parse();

            if (result)
            {
                try
                {
                    if (Source.Attributes != null)
                    {
                        foreach (XmlNode childNode in Source.ChildNodes)
                        {
                            if (childNode.Name == "dataType" || childNode.Name == "dataTypeIDRef")
                            {
                                DataType = new Epos4DataType(childNode, _dataTypeList);

                                if (!DataType.Parse())
                                {
                                    result = false;
                                    break;
                                }
                            }
                            else if (childNode.Name == "flagsIDRef")
                            {
                                if (childNode.Attributes != null)
                                {
                                    var uniqueId = childNode.Attributes["uniqueIDRef"].InnerXml;

                                    Flags = _templateList.FindFlags(uniqueId);
                                }
                            }
                            else if (childNode.Name == "userLevels")
                            {
                                if (!UserLevels.Parse(childNode))
                                {
                                    result = false;
                                    break;
                                }
                            }
                            else if (childNode.Name == "defaultValue")
                            {
                                var defaultValue = new Epos4DefaultValue(childNode, DataType, _dataTypeList);

                                if (!defaultValue.Parse())
                                {
                                    result = false;
                                    break;
                                }

                                DefaultValue = defaultValue;
                                ActualValue = defaultValue.Clone();
                            }
                            else if (childNode.Name == "unit")
                            {
                                var unit = new Epos4Unit();

                                if (!unit.Parse(childNode))
                                {
                                    result = false;
                                    break;
                                }

                                Unit = unit;
                            }
                            else if (childNode.Name == "unitPhysicalQuantityIDRef")
                            {
                                if (childNode.Attributes != null)
                                {
                                    var uniqueIdRef = childNode.Attributes["uniqueIDRef"].InnerXml;

                                    Unit = new Epos4UnitReference { UniqueId = uniqueIdRef };
                                }
                            }
                            else if (childNode.Name == "allowedValues")
                            {
                                var allowedValues = new AllowedValues(_dataTypeList);

                                if (!allowedValues.Parse(childNode))
                                {
                                    result = false;
                                    break;
                                }

                                AllowedValues.Add(allowedValues);
                            }
                            else if (childNode.Name == "allowedValuesIDRef")
                            {
                                if (childNode.Attributes != null)
                                {
                                    var uniqueIdRef = childNode.Attributes["uniqueIDRef"].InnerXml;

                                    var allowedValues = _templateList.FindAllowedValues(uniqueIdRef);

                                    if (allowedValues != null)
                                    {
                                        AllowedValues.Add(allowedValues);
                                    }
                                }
                            }

                            if (!result)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception("Epos4Parameter - Parse", e);
                }
            }
            
            return result;
        }

        #endregion
    }
}
