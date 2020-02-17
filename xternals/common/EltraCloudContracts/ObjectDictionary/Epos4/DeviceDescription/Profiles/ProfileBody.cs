using System.Xml;

using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles
{
    class ProfileBody
    {
        #region Private fields

        private ApplicationProcess _applicationProcess;
        private DeviceManager _deviceManager;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public ProfileBody(Contracts.Devices.EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        private ApplicationProcess ApplicationProcess => _applicationProcess ?? (_applicationProcess = new ApplicationProcess(_device, DeviceManager));

        private DeviceManager DeviceManager => _deviceManager ?? (_deviceManager = new DeviceManager());

        public ParameterList ParameterList => ApplicationProcess.ParameterList;

        public DataRecorderList DataRecorderList => DeviceManager.DataRecorderList;

        #endregion

        #region Methods

        public bool Parse(XmlNode profileBodyNode)
        {
            bool result = true;

            foreach (XmlNode childNode in profileBodyNode.ChildNodes)
            {
                if (childNode.Name == "ApplicationProcess")
                {
                    if (!ApplicationProcess.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                }
                else if (childNode.Name == "DeviceManager")
                {
                    if (!DeviceManager.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                }
            }

            DeviceManager.ResolveParameterReferences(ApplicationProcess.ParameterList);

            return result;
        }

        #endregion
    }
}
