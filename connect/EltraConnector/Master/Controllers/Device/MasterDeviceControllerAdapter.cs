using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraCommon.Transport;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Device;
using EltraConnector.Master.Controllers.Commands;

namespace EltraConnector.Master.Controllers.Device
{
    internal class MasterDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Private fields

        private readonly IHttpClient _httpClient;

        #endregion

        #region Constructors

        public MasterDeviceControllerAdapter(IHttpClient httpClient, string url, Channel channel, UserIdentity userIdentity)
            : base(httpClient, url, channel, userIdentity)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new MasterDeviceCommandsControllerAdapter(_httpClient, Url, Channel, Identity);

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
