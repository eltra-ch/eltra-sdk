using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Epos4;

namespace EltraCloudContracts.ObjectDictionary.Factory
{
    public class ObjectDictionaryFactory
    {
        public static DeviceObjectDictionary CreateObjectDictionary(EltraDevice device)
        {
            DeviceObjectDictionary result = null;

            if (device.Name != "EPOS2")
            {
                result = CreateEpos4ObjectDictionary(device);
            }

            return result;
        }
        
        private static DeviceObjectDictionary CreateEpos4ObjectDictionary(EltraDevice device)
        {
            var obd = new Epos4ObjectDictionary(device);

            return obd;
        }
    }
}
