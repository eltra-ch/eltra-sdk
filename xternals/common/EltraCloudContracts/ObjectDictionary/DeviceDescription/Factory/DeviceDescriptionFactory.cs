using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory
{
    public static class DeviceDescriptionFactory
    {
        public static DeviceDescriptionFile CreateDeviceDescription(EltraDevice device)
        {
            DeviceDescriptionFile result;

            switch (device.Name)
            {
                case "EPOS2":
                    result = new Epos2DeviceDescriptionFile(device);
                    break;
                case "EPOS4":
                case "THERMO":
                    result = new Epos4DeviceDescriptionFile(device);
                    break;
                default:
                    result = new Epos4DeviceDescriptionFile(device);
                    break;
            }

            return result;
        }
    }
}
