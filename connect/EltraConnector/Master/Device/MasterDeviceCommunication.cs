using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using System.Threading.Tasks;
using System.Diagnostics;
using EltraCommon.Logger;

#pragma warning disable 1591

namespace EltraConnector.Master.Device
{
    public class MasterDeviceCommunication
    {
        #region Private fields

        private MasterVcs _vcs;
        
        #endregion

        #region Constructors

        public MasterDeviceCommunication(MasterDevice device)
        {            
            Device = device;

            if (Device != null)
            {
                Device.StatusChanged += OnDeviceStatusChanged;
            }

            Task.Run(() => {
                
                _vcs = new MasterVcs(Device);

            });
            
        }

        #endregion

        #region Properties

        public uint LastErrorCode { get; set; }

        protected MasterDevice Device { get; }

        protected MasterVcs Vcs => _vcs ?? (_vcs = new MasterVcs(Device));

        #endregion

        #region Events

        public event EventHandler Initialized;

        public event EventHandler Finalized;

        public event EventHandler<DeviceCommunicationEventArgs> StatusChanged;

        #endregion

        #region Events handler

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, new EventArgs());
        }

        protected virtual void OnFinalized()
        {
            Finalized?.Invoke(this, new EventArgs());
        }

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            switch(Device.Status)
            {
                case DeviceStatus.Registered:
                    ReadAllParametersAsync();
                    OnInitialized();
                    break;
                case DeviceStatus.Unregistered:
                    OnFinalized();
                    break;
            }

            OnStatusChanged(new DeviceCommunicationEventArgs() { Device = Device, LastErrorCode = LastErrorCode });
        }

        private void ReadAllParametersAsync()
        {
            const string method = "ReadAllParametersAsync";

            Task.Run(async () =>
            {
                if (await WaitForAgent())
                {
                    if(!await Vcs.ReadAllParameters())
                    {
                        MsgLogger.WriteError($"{GetType().Name} - {method}", "Read all parameters failed!");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Wait for agent failed - timeout!");
                }
            });
        }

        private async Task<bool> WaitForAgent()
        {
            int minWaitTime = 10;
            var timeout = new Stopwatch();
            int maxWaitTime = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;

            timeout.Start();

            while (Vcs.Agent == null)
            {
                await Task.Delay(minWaitTime);

                if (timeout.ElapsedMilliseconds > maxWaitTime)
                {
                    break;
                }
            }

            return (Vcs.Agent != null);
        }

        protected virtual void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public virtual bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;
            var parameter = Vcs.SearchParameter(objectIndex, objectSubindex) as Parameter;

            if (parameter != null && parameter.GetValue(out byte[] bytes))
            {
                data = bytes;
                result = true;
            }

            return result;
        }

        public virtual bool GetObject(string loginName, ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            return GetObject(objectIndex, objectSubindex, ref data);
        }

        public virtual bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;
            var xddParameter = Vcs.SearchParameter(objectIndex, objectSubindex) as XddParameter;

            if (xddParameter != null)
            {
                result = xddParameter.SetValue(new ParameterValue(data));
            }

            return result;
        }

        public virtual bool SetObject(string loginName, ushort objectIndex, byte objectSubindex, byte[] data)
        {
            return SetObject(objectIndex, objectSubindex, data);
        }

        protected bool UpdateParameterDictionaryValue<T>(ushort objectIndex, byte objectSubindex, T newValue)
        {
            bool result = false;

            if (Device?.SearchParameter(objectIndex, objectSubindex) is Parameter parameter)
            {
                result = parameter.SetValue(newValue);
            }

            return result;
        }

        #endregion
    }
}
