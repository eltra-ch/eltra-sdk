using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Device;
using EltraConnector.Master.Controllers.Commands;

namespace EltraConnector.Master.Controllers.Device
{
    internal class MasterDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Constructors

        public MasterDeviceControllerAdapter(string url, Channel session, UserIdentity userIdentity)
            : base(url, session, userIdentity)
        {
        }

        #endregion

        #region Properties

        #endregion

        #region Callbacks

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new MasterDeviceCommandsControllerAdapter(Url, Channel, UserIdentity);

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
