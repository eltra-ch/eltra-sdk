﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using EltraCommon.Logger;

using EltraCloudContracts.Contracts.CommandSets;

namespace EltraConnector.Controllers
{
    class DeviceSessionControllerAdapter : SessionControllerAdapter
    {
        #region Private fields

        private DeviceControllerAdapter _deviceControllerAdapter;
        private ParameterControllerAdapter _parameterControllerAdapter;
        private List<EltraDevice> _devices;
        
        #endregion

        #region Constructors

        public DeviceSessionControllerAdapter(string url, UserAuthData authData, uint updateInterval, uint timeout)
            : base(url, authData, updateInterval, timeout)
        {   
        }
        
        #endregion

        #region Properties

        #region Private

        private DeviceControllerAdapter DeviceControllerAdapter => _deviceControllerAdapter ?? (_deviceControllerAdapter = CreateDeviceController());

        private ParameterControllerAdapter ParameterControllerAdapter =>
            _parameterControllerAdapter ?? (_parameterControllerAdapter = CreateParameterControllerAdapter());

        private List<EltraDevice> Devices => _devices ?? (_devices = new List<EltraDevice>());

        private readonly object _locker = new object();

        private EltraDevice[] SafeDevicesArray
        {
            get
            {
                EltraDevice[] devices;

                lock (_locker)
                {
                    devices = Devices.ToArray();
                }

                return devices;
            }
        }

        #endregion

        #endregion

        #region Events handling

        protected virtual void OnDeviceRegistrationStateChanged(object sender, RegistrationEventArgs e)
        {
            var device = e.Device;

            if (e.State == RegistrationState.Registered)
            {
                if (device.DeviceDescription != null)
                { 
                    device.Status = DeviceStatus.Registered;                    
                }
                else
                {
                    device.Status = DeviceStatus.Ready;
                }

                ParameterControllerAdapter.RegisterDevice(device);

                device.RunAsync();
            }
            else if (e.State == RegistrationState.Failed)
            {
                MsgLogger.WriteError($"{GetType().Name} - OnDeviceRegistrationStateChanged", $"Device ({device.Name}) registration failed!");
            }
        }
        
        #endregion

        #region Methods

        private ParameterControllerAdapter CreateParameterControllerAdapter()
        {
            var adapter = new ParameterControllerAdapter(Url, Session);

            AddChild(adapter);

            return adapter;
        }

        private DeviceControllerAdapter CreateDeviceController()
        {
            var adapter = new DeviceControllerAdapter(Url, Session);

            AddChild(adapter);

            RegisterDeviceEvents(adapter);

            return adapter;
        }

        private void RegisterDeviceEvents(DeviceControllerAdapter deviceControllerAdapter)
        {
            deviceControllerAdapter.RegistrationStateChanged += OnDeviceRegistrationStateChanged;
        }

        public async Task<bool> RegisterDevice(EltraDevice device)
        {
            bool result = true;
            
            if (!await IsSessionRegistered())
            {
                if (await RegisterSession())
                {
                    MsgLogger.WriteLine($"register session='{Session.Uuid}' success");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - RegisterDevice", $"register session='{Session.Uuid}' failed!");

                    result = false;
                }
            }

            if (result)
            {
                result = await DeviceControllerAdapter.RegisterDevice(device);
            }

            if (result)
            {
                AddDevice(device);
            }

            return result;
        }

        private void AddDevice(EltraDevice device)
        {
            lock (_locker)
            {
                Devices.Add(device);
            }
        }

        public async Task UnregisterDevice(EltraDevice device)
        {
            await DeviceControllerAdapter.UnregisterSessionDevice(device);

            RemoveDevice(device);
        }

        private void RemoveDevice(EltraDevice device)
        {
            lock (_locker)
            {
                Devices.Remove(device);
            }
        }
        
        private async Task<bool> AnyDeviceUnRegistered()
        {
            bool result = false;

            try
            {
                foreach (var device in SafeDevicesArray)
                {
                    if (!await DeviceControllerAdapter.IsDeviceRegistered(Session.Uuid, device))
                    {
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AnyDeviceUnRegistered", e);
            }
            
            return result;
        }

        public override async Task<bool> Update()
        {
            bool result = false;

            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Is session='{Session.Uuid}' registered...");

                if (await IsSessionRegistered())
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Is any session='{Session.Uuid}' device not registered...");

                    if (await AnyDeviceUnRegistered())
                    {
                        MsgLogger.WriteLine($"re-register session='{Session.Uuid}' devices");

                        if (!await DeviceControllerAdapter.RegisterDevices())
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Session.Uuid}' devices failed!");
                        }
                    }

                    MsgLogger.Write($"{GetType().Name} - Update", $"Updating session='{Session.Uuid}' status...");

                    result = await SetSessionStatus(SessionStatus.Online);
                }
                else
                {
                    MsgLogger.WriteLine($"Registering session='{Session.Uuid}' ...");

                    if (await RegisterSession())
                    {
                        MsgLogger.Write($"{GetType().Name} - Update", $"updating session='{Session.Uuid}' status ...");

                        result = await SetSessionStatus(SessionStatus.Online);

                        if (result)
                        {
                            MsgLogger.WriteLine($"update session='{Session.Uuid}' status success");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"update session='{Session.Uuid}' status failed!");   
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Session.Uuid}' failed!");
                    }

                    if (result)
                    {
                        MsgLogger.WriteLine($"Registering devices='{Session.Uuid}' ...");
                        
                        result = await DeviceControllerAdapter.RegisterDevices();

                        if (result)
                        {
                            MsgLogger.WriteLine($"register session='{Session.Uuid}' devices success");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Session.Uuid}' devices failed!");   
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - AnyDeviceUnRegistered", e);
            }

            return result;
        }

        private async Task<bool> ExecuteCommand(EltraDevice device, ExecuteCommand executeCommand)
        {
            bool result = false;

            try
            {
                if (executeCommand != null)
                {
                    var commandName = executeCommand.Command?.Name;

                    var deviceCommand = device.FindCommand(commandName);

                    if (deviceCommand != null)
                    {
                        if (await DeviceControllerAdapter.SetCommandStatus(executeCommand, ExecCommandStatus.Executing))
                        {
                            try
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Clone Command '{commandName}'");

                                var clonedDeviceCommand = deviceCommand.Clone();

                                if (clonedDeviceCommand != null)
                                {
                                    MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Sync Command '{commandName}'");

                                    clonedDeviceCommand.Sync(executeCommand.Command);

                                    MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Execute Command '{commandName}', session '{executeCommand.SessionUuid}'");

                                    try
                                    {
                                        result = clonedDeviceCommand.Execute(executeCommand.SessionUuid);
                                    }
                                    catch (Exception e)
                                    {
                                        MsgLogger.Exception($"{GetType().Name} - ExecuteCommand", e);
                                    }

                                    if (result)
                                    {
                                        MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Sync Response Command '{commandName}'");

                                        executeCommand.Command?.Sync(clonedDeviceCommand);

                                        MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Push Response for Command '{commandName}'");

                                        result = await DeviceControllerAdapter.PushCommand(executeCommand, ExecCommandStatus.Executed);

                                        if (result)
                                        {
                                            MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Command '{commandName}' successfully processed!");
                                        }
                                        else
                                        {
                                            MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"Command '{commandName}' not found!");
                                        }
                                    }
                                    else
                                    {
                                        var command = executeCommand.Command;

                                        MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand",
                                            command != null
                                                ? $"Command '{command.Name}' uuid '{executeCommand.CommandUuid}' execution failed!"
                                                : $"Command '?' uuid '{executeCommand.CommandUuid}' execution failed!");

                                        await DeviceControllerAdapter.SetCommandStatus(executeCommand,
                                            ExecCommandStatus.Failed);
                                    }
                                }
                                else
                                {
                                    await DeviceControllerAdapter.SetCommandStatus(executeCommand, ExecCommandStatus.Failed);

                                    MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"Command '{commandName}' cloning failed!");
                                }
                            }
                            catch (Exception e)
                            {
                                await DeviceControllerAdapter.SetCommandStatus(executeCommand,
                                    ExecCommandStatus.Failed);

                                MsgLogger.Exception($"{GetType().Name} - ExecuteCommand", e);
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"Set Command '{commandName}' status to executing failed!");
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"Command '{commandName}' not found!");
                    }
                }
            }
            catch(Exception e)
            {
                result = false;
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommand", e);
            }

            return result;
        }

        public async Task<int> ExecuteCommands()
        {                     
            int result = 0;

            try
            {
                int executedCommandCount = 0;

                foreach (var device in SafeDevicesArray)
                {
                    var executeCommands = await DeviceControllerAdapter.PopCommands(device, ExecCommandStatus.Waiting);
                                        
                    foreach (var executeCommand in executeCommands)
                    {
                        if(await ExecuteCommand(device, executeCommand))
                        {
                            executedCommandCount++;
                        }
                    }
                }

                result = executedCommandCount;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommands", e);
            }

            return result;
        }

        private EltraDevice FindDevice(ulong serialNumber)
        {
            EltraDevice result = null;

            foreach (var device in SafeDevicesArray)
            {
                if(device.Identification.SerialNumber == serialNumber)
                {
                    result = device;
                    break;
                }
            }
                            
            return result;
        }

        public async Task<int> ExecuteCommands(List<ExecuteCommand> executeCommands)
        {
            int result = 0;

            try
            {
                int executedCommandCount = 0;
                
                foreach (var executeCommand in executeCommands)
                {
                    var device = FindDevice(executeCommand.SerialNumber);

                    if(device!=null)
                    { 
                        if (await ExecuteCommand(device, executeCommand))
                        {
                            executedCommandCount++;
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ExecuteCommands", $"device with serial number = {executeCommand.SerialNumber} not found!");
                    }
                }
                
                result = executedCommandCount;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommands", e);
            }

            return result;
        }
        
        public async Task<bool> IsDeviceRegistered(EltraDevice device)
        {
            return await DeviceControllerAdapter.IsDeviceRegistered(Session.Uuid, device);
        }

        public override bool Stop()
        {
            _deviceControllerAdapter?.Stop();
            _parameterControllerAdapter?.Stop();

            Task.Run( ()=> UnregisterSession()).GetAwaiter().GetResult();

            return base.Stop();
        }

        #endregion
    }
}