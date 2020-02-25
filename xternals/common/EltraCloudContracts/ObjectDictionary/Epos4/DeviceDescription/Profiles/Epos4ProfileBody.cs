using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles
{
    class Epos4ProfileBody : XddProfileBody
    {
        #region Constructors

        public Epos4ProfileBody(Contracts.Devices.EltraDevice device)
            : base(device)
        {
            
        }

        #endregion

        #region Properties

        public DataRecorderList DataRecorderList => (DeviceManager as Epos4DeviceManager).DataRecorderList;

        #endregion

        #region Methods

        public override bool Parse(XmlNode profileBodyNode)
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
