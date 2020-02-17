using System.Xml;

using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles
{
    class Profile
    {
        #region Private fields

        private ProfileBody _profileBody;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public Profile(Contracts.Devices.EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        private ProfileBody ProfileBody => _profileBody ?? (_profileBody = new ProfileBody(_device));

        public ParameterList ParameterList => ProfileBody.ParameterList;

        public DataRecorderList DataRecorderList => ProfileBody.DataRecorderList;

        #endregion

        #region Methods

        public bool Parse(XmlNode profileNode)
        {
            bool result = false;

            foreach (XmlNode childNode in profileNode.ChildNodes)
            {
                if (childNode.Name == "ProfileBody")
                {
                    result = ProfileBody.Parse(childNode);
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
