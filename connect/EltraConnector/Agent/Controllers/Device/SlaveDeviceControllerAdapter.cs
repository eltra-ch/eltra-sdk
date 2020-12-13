using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Commands;
using EltraConnector.Controllers.Device;

namespace EltraConnector.Agent.Controllers.Device
{
    internal class SlaveDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Constructors

        public SlaveDeviceControllerAdapter(string url, Channel session, UserIdentity userIdentity)
            : base(url, session, userIdentity)
        {
        }

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new SlaveDeviceCommandsControllerAdapter(Url, Channel, UserIdentity);

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
