using System.Xml;

using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Templates;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Units;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application
{
    class ApplicationProcess
    {
        #region Private fields

        private ParameterList _parameterList;
        private DataTypeList _dataTypeList;
        private TemplateList _templateList;
        private Epos4UnitsList _unitsList;
        private readonly DeviceManager _deviceManager;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public ApplicationProcess(Contracts.Devices.EltraDevice device, DeviceManager deviceManager)
        {
            _device = device;
            _deviceManager = deviceManager;
        }

        #endregion
        
        #region Properties

        public ParameterList ParameterList => _parameterList ?? (_parameterList = new ParameterList(_device, _deviceManager, DataTypeList, TemplateList));

        private DataTypeList DataTypeList => _dataTypeList ?? (_dataTypeList = new DataTypeList());
        private TemplateList TemplateList => _templateList ?? (_templateList = new TemplateList(DataTypeList));

        private Epos4UnitsList UnitsList => _unitsList ?? (_unitsList = new Epos4UnitsList(ParameterList));

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
