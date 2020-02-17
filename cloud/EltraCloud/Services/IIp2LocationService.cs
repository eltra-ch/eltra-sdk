using System.Net;

using EltraCloudContracts.Contracts.Sessions;

#pragma warning disable CS1591

namespace EltraCloud.Services
{
    public abstract class IIp2LocationService
    {
        public abstract IpLocation FindAddress(IPAddress address);
    }
}
