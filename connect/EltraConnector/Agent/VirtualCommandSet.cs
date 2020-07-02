using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Threading.Tasks;

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

        #region Methods

        public void RegisterParameterUpdate(string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            if (_connector != null)
            {
                _connector.RegisterParameterUpdate(Device, uniqueId, priority);
            }
        }

        public void UnregisterParameterUpdate(string uniqueId, ParameterUpdatePriority priority = ParameterUpdatePriority.Low)
        {
            if (_connector != null)
            {
                _connector.UnregisterParameterUpdate(Device, uniqueId);
            }
        }

        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            ParameterBase result = null;

            if (_connector != null)
            {
                result = _connector.SearchParameter(Device, index, subIndex);
            }

            return result;
        }

        public ParameterBase SearchParameter(string uniqueId)
        {
            ParameterBase result = null;

            if (_connector != null)
            {
                result = _connector.SearchParameter(Device, uniqueId);
            }

            return result;
        }

        public async Task<Parameter> GetParameter(string uniqueId)
        {
            Parameter result = null;
            
            if(_connector!=null)
            {
                result = await _connector.GetParameter(Device, uniqueId);
            }
            
            return result;
        }

        public async Task<ParameterValue> GetParameterValue(string uniqueId)
        {
            ParameterValue result = null;

            if (_connector != null)
            {
                result = await _connector.GetParameterValue(Device, uniqueId);
            }

            return result;
        }

        public async Task<bool> WriteParameter(Parameter parameter)
        {
            bool result = false;

            if (_connector != null)
            {
                result = await _connector.WriteParameter(Device, parameter);
            }

            return result;
        }

        public async Task<bool> IsLocked()
        {
            bool result = false;

            if (_connector != null)
            {
                result = await _connector.IsDeviceLocked(Device);
            }

            return result;
        }

        public async Task<bool> CanLock()
        {
            bool result = false;

            if (_connector != null)
            {
                result = await _connector.CanLockDevice(Device);
            }

            return result;
        }

        public async Task<bool> Lock()
        {
            bool result = false;

            if (_connector != null)
            {
                result = await _connector.LockDevice(Device);
            }

            return result;
        }

        public async Task<bool> Unlock()
        {
            bool result = false;

            if (_connector != null)
            {
                result = await _connector.UnlockDevice(Device);
            }

            return result;
        }

        #endregion
    }
}
