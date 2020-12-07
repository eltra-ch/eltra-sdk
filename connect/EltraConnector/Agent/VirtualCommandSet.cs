using EltraCommon.Contracts.Devices;
using System;

namespace EltraConnector.Agent
{
    /// <summary>
    /// VirtualCommandSet class
    /// </summary>
    public class VirtualCommandSet
    {
        #region Private fields

        private AgentConnector _connector;
        private EltraDevice _device;

        #endregion

        #region Constructors

        /// <summary>
        /// VirtualCommandSet
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="device"></param>
        public VirtualCommandSet(AgentConnector connector, EltraDevice device)
        {
            _connector = connector;
            _device = device;

            _device.StatusChanged += OnDeviceStatusChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Device status changed
        /// </summary>
        public event EventHandler DeviceStatusChanged;
        
        /// <summary>
        /// Device changed
        /// </summary>
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

        /// <summary>
        /// Agent connector instance
        /// </summary>
        public AgentConnector Connector => _connector;
        
        /// <summary>
        /// Device
        /// </summary>
        public EltraDevice Device
        {
            get => _device;
            set
            {
                _device = value;

                OnDeviceChanged();
            }
        }

        /// <summary>
        /// Device status
        /// </summary>
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
