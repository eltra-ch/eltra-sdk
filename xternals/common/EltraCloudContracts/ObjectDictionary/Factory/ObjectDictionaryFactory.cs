using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Epos4;
using EltraCloudContracts.ObjectDictionary.Xdd;
using System;

namespace EltraCloudContracts.ObjectDictionary.Factory
{
    public class ObjectDictionaryFactory
    {
        public static DeviceObjectDictionary CreateObjectDictionary(EltraDevice device)
        {
            DeviceObjectDictionary result = null;

            if (device.Name == "EPOS2")
            {
                throw new NotImplementedException();
            }
            else if (device.Name == "EPOS4")
            {
                result = new Epos4ObjectDictionary(device);
            }
            else
            {
                result = new XddObjectDictionary(device);
            }

            return result;
        }        
    }
}
