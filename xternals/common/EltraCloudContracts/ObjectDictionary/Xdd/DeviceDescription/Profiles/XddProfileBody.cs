using System.Xml;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Identity;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles
{
    public class XddProfileBody
    {
        #region Private fields

        private XddDeviceIdentity _deviceIdentity;
        private XddApplicationProcess _applicationProcess;
        private XddDeviceManager _deviceManager;
        private readonly EltraDevice _device;

        #endregion

        #region Constructors

        public XddProfileBody(EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        protected XddApplicationProcess ApplicationProcess => _applicationProcess ?? (_applicationProcess = new XddApplicationProcess(_device, DeviceManager));

        protected XddDeviceManager DeviceManager => _deviceManager ?? (_deviceManager = new XddDeviceManager());

        protected XddDeviceIdentity DeviceIdentity => _deviceIdentity ?? (_deviceIdentity = new XddDeviceIdentity(_device));

        public XddParameterList ParameterList => ApplicationProcess.ParameterList;

        #endregion

        #region Methods

        public virtual bool Parse(XmlNode profileBodyNode)
        {
            bool result = true;

            foreach (XmlNode childNode in profileBodyNode.ChildNodes)
            {
                if (childNode.Name == "DeviceIdentity")
                {
                    if (!DeviceIdentity.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                }
                else if (childNode.Name == "ApplicationProcess")
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
