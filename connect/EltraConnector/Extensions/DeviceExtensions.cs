using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.Agent;

namespace EltraConnector.Extensions
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

        public static ParameterBase SearchParameter(this EltraDevice device, AgentConnector connector, string uniqueId)
        {
            ParameterBase result = null;

            if (connector != null)
            {
                result = connector.SearchParameter(device, uniqueId);
            }

            return result;
        }
    }
}
