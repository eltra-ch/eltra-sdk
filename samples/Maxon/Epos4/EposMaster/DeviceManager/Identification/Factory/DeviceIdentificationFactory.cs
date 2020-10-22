using EposMaster.DeviceManager.Device;

namespace EposMaster.DeviceManager.Identification.Factory
{
    static class DeviceIdentificationFactory
    {
        public static EposDeviceIdentification CreateIdentification(EposDevice device)
        {
            EposDeviceIdentification result = null;

            switch (device.Family)
            {
                case "EPOS2":
                    result = new Epos2DeviceIdentification(device);
                    break;
                case "EPOS4":
                    result = new Epos4DeviceIdentification(device);
                    break;
            }

            return result;
        }
    }
}
