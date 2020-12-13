using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Users;
using EltraConnector.Controllers;
using EltraConnector.Controllers.Commands;
using EltraConnector.Controllers.Device;
using EltraConnector.Transport.Udp;

namespace EltraConnector.Master.Controllers.Device
{
    internal class MasterDeviceControllerAdapter : DeviceControllerAdapter
    {
        #region Private fields

        private EltraUdpServer _udpServer;

        #endregion

        #region Constructors

        public MasterDeviceControllerAdapter(string url, Channel session, UserIdentity userIdentity)
            : base(url, session, userIdentity)
        {
        }

        #endregion

        #region Properties

        public EltraUdpServer UdpServer
        {
            get => _udpServer;
            set
            {
                _udpServer = value;
                OnUdpServerChanged();
            }
        }

        #endregion

        #region Callbacks

        private void OnUdpServerChanged()
        {
            if(DeviceCommandsAdapter is MasterDeviceCommandsControllerAdapter adapter)
            {
                adapter.UdpServer = UdpServer;
            }
        }

        #endregion

        #region Methods

        protected override DeviceCommandsControllerAdapter CreateDeviceCommandsAdapter()
        {
            var adapter = new MasterDeviceCommandsControllerAdapter(Url, Channel, UserIdentity);

            adapter.UdpServer = UdpServer;

            AddChild(adapter);

            return adapter;
        }

        #endregion
    }
}
