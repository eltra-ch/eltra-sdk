using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription;

namespace EltraCloudContracts.ObjectDictionary.Common
{
    public class XddDeviceObjectDictionary : DeviceObjectDictionary
    {
        #region Constructors

        public XddDeviceObjectDictionary(EltraDevice device)
            : base(device)
        {
        }

        protected override bool CreateDeviceDescription()
        {
            bool result = false;
            var deviceDescription = Device?.DeviceDescription;
            var content = deviceDescription?.Content;

            if (content != null)
            {
                var xdd = new Xdd(Device) { DataSource = content };

                SetDeviceDescription(xdd);

                result = true;
            }

            return result;
        }

        #endregion
    }
}