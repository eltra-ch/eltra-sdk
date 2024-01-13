using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.Transport;
using EltraConnector.Agent.Controllers.Commands;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Device;
using EltraConnector.Transport.Udp;
using System.Net.Sockets;

namespace EltraConnector.Agent.Controllers.Device
{
    internal class SlaveDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Private fields

        private readonly IHttpClient _httpClient;
        private readonly IUdpClient _udpClient;

        #endregion

        #region Constructors

        public SlaveDeviceControllerAdapter(IHttpClient httpClient, IUdpClient udpClient, string url, Channel channel, UserIdentity userIdentity)
            : base(httpClient, url, channel, userIdentity)
        {
            _udpClient = udpClient;
            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new SlaveDeviceCommandsControllerAdapter(_httpClient, _udpClient, Url, Channel, Identity);

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
