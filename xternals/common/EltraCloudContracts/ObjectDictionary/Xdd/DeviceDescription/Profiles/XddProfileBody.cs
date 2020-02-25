using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles
{
    public class XddProfileBody
    {
        #region Private fields

        private XddApplicationProcess _applicationProcess;
        private XddDeviceManager _deviceManager;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public XddProfileBody(Contracts.Devices.EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        protected XddApplicationProcess ApplicationProcess => _applicationProcess ?? (_applicationProcess = new XddApplicationProcess(_device, DeviceManager));

        protected XddDeviceManager DeviceManager => _deviceManager ?? (_deviceManager = new XddDeviceManager());

        public XddParameterList ParameterList => ApplicationProcess.ParameterList;

        #endregion

        #region Methods

        public virtual bool Parse(XmlNode profileBodyNode)
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
