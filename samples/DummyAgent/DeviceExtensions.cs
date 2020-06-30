using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace DummyAgent
{
    public static class DeviceExtensions
    {
        public static ParameterBase SearchParameter(this EltraDevice device, AgentConnector connector, ushort index, byte subIndex)
        {
            ParameterBase result = null;

            if (connector != null)
            {
                result = connector.SearchParameter(device, index, subIndex);
            }

            return result;
        }
    }
}
