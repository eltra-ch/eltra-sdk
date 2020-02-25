using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Templates;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application
{
    public class XddApplicationProcess
    {
        #region Private fields

        private XddParameterList _parameterList;
        private XddDataTypeList _dataTypeList;
        private XddTemplateList _templateList;
        private XddUnitsList _unitsList;
        private readonly XddDeviceManager _deviceManager;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public XddApplicationProcess(Contracts.Devices.EltraDevice device, XddDeviceManager deviceManager)
        {
            _device = device;
            _deviceManager = deviceManager;
        }

        #endregion
        
        #region Properties

        public XddParameterList ParameterList => _parameterList ?? (_parameterList = new XddParameterList(_device, _deviceManager, DataTypeList, TemplateList));

        private XddDataTypeList DataTypeList => _dataTypeList ?? (_dataTypeList = new XddDataTypeList());
        private XddTemplateList TemplateList => _templateList ?? (_templateList = new XddTemplateList(DataTypeList));

        private XddUnitsList UnitsList => _unitsList ?? (_unitsList = new XddUnitsList(ParameterList));

        #endregion

        #region Methods

        public bool Parse(XmlNode applicationProcessNode)
        {
            bool result = false;
            
            foreach (XmlNode childNode in applicationProcessNode.ChildNodes)
            {
                if (childNode.Name == "dataTypeList")
                {
                    result = DataTypeList.Parse(childNode);
                }
                else if (childNode.Name == "unitsList")
                {
                    result = UnitsList.Parse(childNode);
                }
                else if (childNode.Name == "parameterList")
                {
                    ParameterList.UnitsList = UnitsList;

                    result = ParameterList.Parse(childNode);
                }
                else if (childNode.Name == "templateList")
                {
                    result = TemplateList.Parse(childNode);
                }
            }

            return result;
        }

        #endregion
    }
}
