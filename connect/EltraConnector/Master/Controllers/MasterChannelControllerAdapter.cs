using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EltraConnector.Controllers.Base;
using EltraConnector.Events;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Contracts.CommandSets;
using EltraConnector.SyncAgent;
using EltraConnector.Controllers.Events;
using EltraCommon.Contracts.ToolSet;
using EltraConnector.Controllers.Device;
using EltraConnector.Controllers;
using EltraConnector.Master.Controllers.Device;
using EltraConnector.Master.Controllers.Commands;

namespace EltraConnector.Master.Controllers
{
    class MasterChannelControllerAdapter : ChannelControllerAdapter
    {
        #region Private fields

        private readonly SyncCloudAgent _agent;
        private readonly ExecuteCommandCache _executeCommandCache;

        private DeviceControllerAdapter _deviceControllerAdapter;
        private ParameterControllerAdapter _parameterControllerAdapter;
        private List<EltraDevice> _devices;
        

        #endregion

        #region Constructors

        public MasterChannelControllerAdapter(SyncCloudAgent agent)
            : base(agent.Url, agent.Identity, agent.UpdateInterval, agent.Timeout)
        {
            _executeCommandCache = new ExecuteCommandCache();

            _agent = agent;
        }

        public MasterChannelControllerAdapter(SyncCloudAgent agent, string channelId)
            : base(agent.Url, channelId, agent.Identity, agent.UpdateInterval, agent.Timeout)
        {
            _executeCommandCache = new ExecuteCommandCache();

            _agent = agent;
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
                ParameterControllerAdapter.ParametersUpdated -= OnParametersUpdated;
                ParameterControllerAdapter.ParametersUpdated += OnParametersUpdated;

                ParameterControllerAdapter.RegisterDevice(device);                
            }
            else if (e.State == RegistrationState.Failed)
            {
                MsgLogger.WriteError($"{GetType().Name} - OnDeviceRegistrationStateChanged", $"Device ({device.Family}) registration failed!");
            }
        }

        private void OnParametersUpdated(object sender, ParameterUpdateEventArgs args)
        {
            var device = args.Device;

            MsgLogger.WriteFlow($"{GetType().Name} - OnParametersUpdated", $"Device = {device.Name}, parameters updated, result = {args.Result}");

            if (device != null)
            {
                if (device.DeviceDescription != null)
                {
                    device.Status = DeviceStatus.Registered;
                }
                else
                {
                    device.Status = DeviceStatus.Ready;
                }

                device.RunAsync();
            }
        }

        #endregion

        #region Methods

        internal Task<bool> PayloadExists(DeviceToolPayload payload)
        {
            return DeviceControllerAdapter.PayloadExists(payload);
        }

        internal Task<bool> UploadPayload(DeviceToolPayload payload)
        {
            return DeviceControllerAdapter.UploadPayload(payload);
        }

        private ParameterControllerAdapter CreateParameterControllerAdapter()
        {
            var adapter = new ParameterControllerAdapter(_agent.Identity, Url, Channel);

            AddChild(adapter);

            return adapter;
        }

        private DeviceControllerAdapter CreateDeviceController()
        {
            var adapter = new MasterDeviceControllerAdapter(Url, Channel, _agent.Identity) 
            { 
                ConnectionManager = ConnectionManager
            };

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
            
            if (!await IsChannelRegistered())
            {
                if (await RegisterChannel())
                {
                    MsgLogger.WriteLine($"register session='{Channel.Id}' success");
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - RegisterDevice", $"register session='{Channel.Id}' failed!");

                    result = false;
                }
            }

            if (result)
            {
                if (!await IsDeviceRegistered(device))
                {
                    result = await DeviceControllerAdapter.RegisterDevice(device);
                }
                else
                {
                    result = await DeviceControllerAdapter.UpdateDevice(device);
                }
            }

            if (result)
            {
                if (FindDevice(device.NodeId) == null)
                {
                    AddDevice(device);
                }
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

        public async Task<bool> UnregisterDevice(EltraDevice device)
        {
            bool result = await DeviceControllerAdapter.UnregisterChannelDevice(device);

            if (result)
            {
                RemoveDevice(device);
            }

            return result;
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
                foreach (var deviceNode in SafeDevicesArray)
                {
                    var status = await DeviceControllerAdapter.GetDeviceStatus(deviceNode);

                    if (status != DeviceStatus.Registered && status != DeviceStatus.Ready)
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
                MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Is session='{Channel.Id}' registered...");

                if (await IsChannelRegistered())
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Is any session='{Channel.Id}' device not registered...");

                    if (await AnyDeviceUnRegistered())
                    {
                        MsgLogger.WriteLine($"re-register session='{Channel.Id}' devices");

                        if (!await DeviceControllerAdapter.RegisterDevices())
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Channel.Id}' devices failed!");
                        }
                    }

                    MsgLogger.Write($"{GetType().Name} - Update", $"Updating session='{Channel.Id}' status...");

                    result = await SetChannelStatus(ChannelStatus.Online);
                }
                else
                {
                    MsgLogger.WriteLine($"Registering session='{Channel.Id}' ...");

                    if (await RegisterChannel())
                    {
                        MsgLogger.Write($"{GetType().Name} - Update", $"updating session='{Channel.Id}' status ...");

                        result = await SetChannelStatus(ChannelStatus.Online);

                        if (result)
                        {
                            MsgLogger.WriteLine($"update session='{Channel.Id}' status success");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"update session='{Channel.Id}' status failed!");   
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Channel.Id}' failed!");
                    }

                    if (result)
                    {
                        MsgLogger.WriteLine($"Registering devices='{Channel.Id}' ...");
                        
                        result = await DeviceControllerAdapter.RegisterDevices();

                        if (result)
                        {
                            MsgLogger.WriteLine($"register session='{Channel.Id}' devices success");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Update", $"register session='{Channel.Id}' devices failed!");   
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
                if (executeCommand != null && _executeCommandCache.CanExecute(executeCommand))
                {
                    var s = MsgLogger.BeginTimeMeasure();

                    var sourceChannelId = executeCommand.SourceChannelId;
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

                                    MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Execute Command '{commandName}', session '{executeCommand.SourceChannelId}'");

                                    try
                                    {
                                        var start = MsgLogger.BeginTimeMeasure();

                                        result = clonedDeviceCommand.Execute(executeCommand.SourceChannelId, executeCommand.SourceLoginName);

                                        MsgLogger.EndTimeMeasure($"{GetType().Name} - ExecuteCommand", start, $"command '{executeCommand.Command.Name}' executed, result = {result}");
                                    }
                                    catch (Exception e)
                                    {
                                        MsgLogger.Exception($"{GetType().Name} - ExecuteCommand", e);
                                    }

                                    if (result)
                                    {
                                        var start = MsgLogger.BeginTimeMeasure();

                                        MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Sync Response Command '{commandName}'");

                                        executeCommand.Command?.Sync(clonedDeviceCommand);

                                        MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Push Response for Command '{commandName}'");

                                        executeCommand.SourceChannelId = Channel.Id;
                                        executeCommand.TargetChannelId = sourceChannelId;

                                        result = await DeviceControllerAdapter.PushCommand(executeCommand, ExecCommandStatus.Executed);

                                        if (result)
                                        {
                                            MsgLogger.WriteDebug($"{GetType().Name} - ExecuteCommand", $"Command '{commandName}' successfully processed!");
                                        }
                                        else
                                        {
                                            MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand", $"Set command '{commandName}' status to exectuted failed!");
                                        }

                                        MsgLogger.EndTimeMeasure($"{GetType().Name} - ExecuteCommand", start, $"command '{executeCommand.Command.Name}' state synchronized, result = {result}");
                                    }
                                    else
                                    {
                                        var command = executeCommand.Command;

                                        MsgLogger.WriteError($"{GetType().Name} - ExecuteCommand",
                                            command != null
                                                ? $"Command '{command.Name}' uuid '{executeCommand.CommandId}' execution failed!"
                                                : $"Command '?' uuid '{executeCommand.CommandId}' execution failed!");

                                        executeCommand.SourceChannelId = Channel.Id;
                                        executeCommand.TargetChannelId = sourceChannelId;

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

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - ExecuteCommand", s, $"command '{executeCommand.Command?.Name}' executed, result = {result}");
                }
            }
            catch(Exception e)
            {
                result = false;
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommand", e);
            }

            return result;
        }

        public async Task<bool> ExecuteCommands()
        {                     
            bool result = true;

            try
            {
                foreach (var deviceNode in SafeDevicesArray)
                {
                    var executeCommands = await DeviceControllerAdapter.PopCommands(deviceNode, ExecCommandStatus.Waiting);
                    
                    foreach (var executeCommand in executeCommands)
                    {
                        if(!await ExecuteCommand(deviceNode, executeCommand))
                        {
                            result = false;
                        }
                    }

                    if (result)
                    {
                        result = DeviceControllerAdapter.Good;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommands", e);
                result = false;
            }

            return result;
        }

        private EltraDevice FindDevice(int nodeId)
        {
            EltraDevice result = null;

            foreach (var deviceNode in SafeDevicesArray)
            {
                var device = deviceNode;
                
                if(device.NodeId == nodeId)
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

            var start = MsgLogger.BeginTimeMeasure();

            try
            {
                int executedCommandCount = 0;
                
                foreach (var executeCommand in executeCommands)
                {
                    var device = FindDevice(executeCommand.NodeId);

                    if(device!=null)
                    { 
                        if (await ExecuteCommand(device, executeCommand))
                        {
                            executedCommandCount++;
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ExecuteCommands", $"device with node id = {executeCommand.NodeId} not found!");
                    }
                }
                
                result = executedCommandCount;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ExecuteCommands", e);
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - ExecuteCommands", start, $"commands executed, count = {executeCommands.Count}");

            return result;
        }
        
        public async Task<bool> IsDeviceRegistered(EltraDevice device)
        {
            var status = await DeviceControllerAdapter.GetDeviceStatus(device);

            return status == DeviceStatus.Registered || status == DeviceStatus.Ready;
        }

        public override bool Stop()
        {
            _deviceControllerAdapter?.Stop();
            _parameterControllerAdapter?.Stop();
            
            Task.Run( ()=> UnregisterChannel()).GetAwaiter().GetResult();

            return base.Stop();
        }

        #endregion
    }
}