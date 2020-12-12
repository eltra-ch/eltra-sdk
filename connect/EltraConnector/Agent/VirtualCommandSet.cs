using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Channels.Events;
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
        private Channel _channel;

        #endregion

        #region Constructors

        /// <summary>
        /// VirtualCommandSet
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="device"></param>
        public VirtualCommandSet(AgentConnector connector, EltraDevice device)
        {
            if (connector != null)
            {
                _connector = connector;

                if (device != null)
                {
                    _channel = _connector.GetChannel(device);

                    if(_channel!=null)
                    {
                        _channel.StatusChanged += OnDeviceChannelStatusChanged;
                    }

                    _device = device;

                    _device.StatusChanged += OnDeviceStatusChanged;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Device status changed
        /// </summary>
        public event EventHandler DeviceStatusChanged;

        /// <summary>
        /// Device channel status changed
        /// </summary>
        public event EventHandler<ChannelStatusChangedEventArgs> DeviceChannelStatusChanged;

        /// <summary>
        /// Device changed
        /// </summary>
        public event EventHandler DeviceChanged;

        #endregion

        #region Event handler

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            DeviceStatusChanged?.Invoke(sender, e);
        }

        private void OnDeviceChanged()
        {
            DeviceChanged?.Invoke(this, new EventArgs());
        }

        private void OnDeviceChannelStatusChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            DeviceChannelStatusChanged?.Invoke(sender, e);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Agent connector instance
        /// </summary>
        public AgentConnector Connector => _connector;

        /// <summary>
        /// Channel
        /// </summary>
        public Channel Channel => _channel;
        
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
                var result = DeviceStatus.Undefined;

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
