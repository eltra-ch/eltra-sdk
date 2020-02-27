using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription;
using System;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory
{
    public static class DeviceDescriptionFactory
    {
        public static DeviceDescriptionFile CreateDeviceDescriptionFile(EltraDevice device)
        {
            DeviceDescriptionFile result;

            switch (device.Name)
            {
                case "EPOS2":
                    result = new Epos2DeviceDescriptionFile(device);
                    break;
                case "EPOS4":
                    result = new XddDeviceDescriptionFile(device);
                    break;
                default:
                    result = new XddDeviceDescriptionFile(device);
                    break;
            }

            return result;
        }

        public static Dd CreateDeviceDescription(EltraDevice device, DeviceDescriptionFile deviceDescriptionFile)
        {
            Dd result = null;
            var content = deviceDescriptionFile?.Content;

            if (content != null && device != null)
            {
                switch (device.Name)
                {
                    case "EPOS2":
                        throw new NotImplementedException();
                    case "EPOS4":
                        result = new XddDeviceDescription(device) { DataSource = content };
                        break;
                    default:
                        result = new XddDeviceDescription(device) { DataSource = content };
                        break;
                }
            }

            return result;
        }
    }
}
