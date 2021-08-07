using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraConnector.Agent.Controllers.Commands;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Device;

namespace EltraConnector.Agent.Controllers.Device
{
    internal class SlaveDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Constructors

        public SlaveDeviceControllerAdapter(string url, Channel channel, UserIdentity userIdentity)
            : base(url, channel, userIdentity)
        {
        }

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new SlaveDeviceCommandsControllerAdapter(Url, Channel, Identity);

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
