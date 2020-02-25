using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles
{
    class Epos4Profile : XddProfile
    {
        #region Constructors

        public Epos4Profile(Contracts.Devices.EltraDevice device)
            : base(device)
        {
            ProfileBody = new Epos4ProfileBody(device);
        }

        #endregion

        #region Properties

        public DataRecorderList DataRecorderList => (ProfileBody as Epos4ProfileBody).DataRecorderList;

        #endregion
    }
}
