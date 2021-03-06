﻿using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace EltraConnector.Master.Device
{
    public class MasterDeviceManager : IDisposable
    {
        #region Private fields

        private List<MasterDevice> _deviceList;
        private SyncCloudAgent _cloudAgent;

        #endregion

        #region Constructors

        public MasterDeviceManager()
        {
        }

        public MasterDeviceManager(SyncCloudAgent cloudAgent)
        {
            CloudAgent = cloudAgent;
        }

        #endregion

        #region Properties

        public SyncCloudAgent CloudAgent
        {
            get => _cloudAgent;
            set
            {
                _cloudAgent = value;

                OnCloudAgentChanged();
            }
        }

        protected List<MasterDevice> DeviceList
        {
            get => _deviceList ?? (_deviceList = new List<MasterDevice>());
        }

        #endregion

        #region Methods

        protected virtual void OnCloudAgentChanged()
        {
            foreach(var device in DeviceList)
            {
                device.CloudAgent = CloudAgent;
            }
        }

        public void AddDevice(MasterDevice device)
        {
            if (device != null)
            {
                if (CloudAgent != null)
                {
                    device.CloudAgent = CloudAgent;
                }

                DeviceList.Add(device);
            }
        }

        protected virtual async void Dispose(bool finalize)
        {
            if (finalize)
            {
                await Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public virtual async Task Stop()
        {
            foreach(var device in DeviceList)
            {
                device.Disconnect();

                MsgLogger.WriteLine($"{GetType().Name} - Stop", $"Disconnected: device = {device.Family} - Unregister");

                if(await CloudAgent.UnregisterDevice(device))
                {
                    device.Status = DeviceStatus.Unregistered;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Stop", "UnregisterDevice failed!");
                }                
            }
        }

        public virtual async Task<bool> Run()
        {
            bool result = true;
            
            MsgLogger.WriteLine($"{GetType().Name} - Run", $"Run: device count='{DeviceList.Count}'");

            foreach (var device in DeviceList)
            {
                if (device.Status == DeviceStatus.Ready)
                {
                    MsgLogger.WriteLine($"{GetType().Name} - Run", $"Register: device='{device.Family}', node id = {device.NodeId}, serial number=0x{device.Identification.SerialNumber:X}");

                    if(!await CloudAgent.RegisterDevice(device))
                    {
                        result = false;
                    }
                }
            }

            foreach (var device in DeviceList)
            {
                device.StatusChanged += async (o, e) =>
                {
                    if (device.Status == DeviceStatus.Ready)
                    {
                        MsgLogger.WriteLine($"{GetType().Name} - Run", $"Register: device='{device.Family}', node id = {device.NodeId}, serial number=0x{device.Identification.SerialNumber:X}");

                        if(!await CloudAgent.RegisterDevice(device))
                        {
                            result = false;
                        }
                    }
                };
            }

            return result;
        }

        #endregion
    }
}
