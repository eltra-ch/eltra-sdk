
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd
{
    public class XddObjectDictionary : DeviceObjectDictionary
    {
        public XddObjectDictionary(EltraDevice device)
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
                var xdd = new DeviceDescription.XddDeviceDescription(Device) { DataSource = content };

                SetDeviceDescription(xdd);

                result = true;
            }

            return result;
        }
    }
}
