using EltraCloudContracts.Contracts.Devices;
using System;

namespace EltraConnector.Agent
{
    public class VirtualCommandSet
    {
        #region Private fields

        private AgentConnector _connector;
        private EltraDevice _device;

        #endregion

        #region Constructors

        public VirtualCommandSet(AgentConnector connector, EltraDevice device)
        {
            _connector = connector;
            _device = device;

            _device.StatusChanged += OnDeviceStatusChanged;
        }

        #endregion

        #region Events

        public event EventHandler DeviceStatusChanged;
        public event EventHandler DeviceChanged;

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            DeviceStatusChanged?.Invoke(sender, e);
        }

        private void OnDeviceChanged()
        {
            DeviceChanged?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Properties

        protected AgentConnector Connector => _connector;
        
        public EltraDevice Device
        {
            get => _device;
            set
            {
                _device = value;

                OnDeviceChanged();
            }
        }

        public DeviceStatus DeviceStatus
        {
            get
            {
                DeviceStatus result = DeviceStatus.Undefined;

                if(_device!=null)
                {
                    result = _device.Status;
                }

                return result;
            }
        }

        #endregion
    }
}
