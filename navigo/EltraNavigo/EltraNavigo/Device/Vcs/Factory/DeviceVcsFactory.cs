using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;

namespace EltraNavigo.Device.Vcs.Factory
{
    class DeviceVcsFactory
    {
        public static DeviceVcs CreateVcs(DeviceAgent agent, EltraCloudContracts.Contracts.Devices.EltraDevice device)
        {
            DeviceVcs vcs = null;

            switch (device.Family)
            {
                case "EPOS4":
                {
                    vcs = new EposVcs(agent, device);
                } break;
                case "4 Channel Relay":
                {
                    vcs = new RelayVcs(agent, device);
                } break;
                case "THERMO":
                {
                    vcs = new ThermoVcs(agent, device);
                }    
                break;
                case "PHOTO":
                    {
                        vcs = new PhotoVcs(agent, device);
                    }
                    break;
            }

            return vcs;
        }
    }
}
