using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using Newtonsoft.Json;

using EltraConnector.Controllers.Base;
using EltraConnector.Extensions;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.Helpers;
using EltraCommon.Contracts.Devices;
using EltraConnector.Transport.Ws;
using EltraCommon.Contracts.Users;

namespace EltraConnector.Controllers
{
    internal class DeviceCommandsControllerAdapter : CloudChannelControllerAdapter
    {
        #region Private fields
                
        private readonly UserIdentity _userIdentity;        
        private string _commandExecUuid;
        private WsConnectionManager _wsConnectionManager;
        private bool _master;

        #endregion

        #region Constructors

        public DeviceCommandsControllerAdapter(string url, Channel channel, UserIdentity userIdentity, bool master)
            : base(url, channel)
        {
            _userIdentity = userIdentity;
            _master = master;

            if (!master)
            {
                _commandExecUuid = channel.Id + "_ExecCommander";
            }
        }

        #endregion

        #region Properties

        public WsConnectionManager WsConnectionManager 
        { 
            get => _wsConnectionManager;
            set
            {
                _wsConnectionManager = value;

                OnWsConnectionManagerChanged();
            } 
        }

        #endregion

        #region Events handling

        private void OnWsConnectionManagerChanged()
        {
        }

        #endregion

        #region Methods

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice deviceNode)
        {
            List<DeviceCommand> result = null;
            var device = deviceNode;

            if (device != null)
            {
                MsgLogger.WriteLine($"get device='{device.Family}', nodeId=0x{device.NodeId} commands");

                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["callerId"] = Channel.Id;
                    query["nodeId"] = $"{device.NodeId}";

                    var url = UrlHelper.BuildUrl(Url, "api/command/commands", query);

                    var json = await Transporter.Get(_userIdentity, url);

                    var commandSet = JsonConvert.DeserializeObject<DeviceCommandSet>(json);

                    if (commandSet != null)
                    {
                        AssignDeviceToCommand(deviceNode, commandSet);

                        result = commandSet.Commands;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetDeviceCommands", e);
                }
            }

            return result;
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDevice deviceNode, string commandName)
        {
            DeviceCommand result = null;
            var device = deviceNode;

            if (device != null)
            {
                MsgLogger.WriteLine($"get command '{commandName}' from device='{device.Family}', node id={device.NodeId}");

                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["callerId"] = device.ChannelId;
                    query["nodeId"] = $"{device.NodeId}";
                    query["commandName"] = $"{commandName}";

                    var url = UrlHelper.BuildUrl(Url, "api/command/command", query);

                    var json = await Transporter.Get(_userIdentity, url);

                    var command = JsonConvert.DeserializeObject<DeviceCommand>(json);

                    if (command != null)
                    {
                        command.Device = deviceNode;

                        result = command;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetDeviceCommand", e);
                }
            }

            return result;
        }

        private static void AssignDeviceToCommand(EltraDevice device, DeviceCommandSet result)
        {
            if (result != null)
            {
                foreach (var command in result.Commands)
                {
                    command.Device = device;
                }
            }
        }

        public async Task<bool> PushCommand(ExecuteCommand execCommand)
        {
            bool result = false;

            try
            {
                var device = execCommand?.Command?.Device;

                if (device != null)
                {
                    MsgLogger.WriteLine($"push command='{execCommand.Command.Name}' to device='{device.Family}':0x{device.NodeId}");

                    if(_master)
                    {
                        _commandExecUuid = device.ChannelId + $"_ExecCommander_{device.NodeId}";
                    }

                    if (WsConnectionManager != null && WsConnectionManager.IsConnected(_commandExecUuid))
                    {
                        if(await WsConnectionManager.Send(_commandExecUuid, _userIdentity, execCommand))
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var postResult = await Transporter.Post(_userIdentity, Url, "api/command/push", execCommand.ToJson());

                        if (postResult.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;
                        }
                    }                    
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PushCommand", e);
            }

            return result;
        }

        public async Task<bool> PushCommand(DeviceCommand command, string agentUuid, ExecCommandStatus status)
        {
            bool result = false;
            var device = command?.Device;
                        
            if (device != null)
            {
                var execCommand = new ExecuteCommand { Command = command, 
                                                       NodeId = device.NodeId,
                                                       TargetChannelId = device.ChannelId,
                                                       SourceChannelId = agentUuid };

                command.Status = status;

                result = await PushCommand(execCommand);
            }

            return result;
        }

        public async Task<bool> SetCommandStatus(ExecuteCommandStatus status)
        {
            bool result = false;

            try
            {
                MsgLogger.WriteLine($"set command='{status.CommandName}' status='{status.Status}' for device with nodeid={status.NodeId}");

                var postResult = await Transporter.Post(_userIdentity, Url, "api/command/status", JsonConvert.SerializeObject(status));

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SetCommandStatus", e);
            }

            return result;
        }

        public async Task<bool> SetCommandStatus(ExecuteCommand executeCommand, ExecCommandStatus status)
        {
            var execCommandStatus = new ExecuteCommandStatus(Channel.Id, executeCommand) { Status = status };

            return await SetCommandStatus(execCommandStatus);
        }

        public async Task<List<ExecuteCommand>> PullCommands(EltraDevice deviceNode, ExecCommandStatus status)
        {
            var result = new List<ExecuteCommand>();
            var device = deviceNode;

            if (device != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["sourceChannelId"] = Channel.Id;
                    query["targetChannelId"] = $"{deviceNode.ChannelId}";
                    query["nodeId"] = $"{device.NodeId}";
                    query["status"] = $"{status}";
                    
                    var url = UrlHelper.BuildUrl(Url, "api/command/pull", query);

                    var json = await Transporter.Get(_userIdentity, url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var executeCommands = JsonConvert.DeserializeObject<List<ExecuteCommand>>(json);

                        foreach (var executeCommand in executeCommands)
                        {
                            if (executeCommand?.Command != null)
                            {
                                executeCommand.Command.Device = deviceNode;
                            }
                        }

                        result = executeCommands;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - PopCommands", e);
                }
            }

            return result;
        }

        public async Task<ExecuteCommand> PopCommand(string commandUuid, EltraDevice deviceNode, ExecCommandStatus status)
        {
            ExecuteCommand result = null;
            var device = deviceNode;

            if (device != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["callerId"] = Channel.Id;
                    query["commandId"] = $"{commandUuid}";
                    query["nodeId"] = $"{device.NodeId}";
                    query["status"] = $"{status}";
                    
                    var url = UrlHelper.BuildUrl(Url, "api/command/pop", query);

                    var json = await Transporter.Get(_userIdentity, url);

                    result = JsonConvert.DeserializeObject<ExecuteCommand>(json);

                    if (result?.Command != null)
                    {
                        result.Command.Device = deviceNode;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - PopCommand", e);
                }
            }

            return result;
        }

        public async Task<ExecuteCommandStatus> GetCommandStatus(string uuid, ExecuteCommand executeCommand)
        {
            ExecuteCommandStatus result = null;
            
            try
            {
                var commandName = executeCommand.Command.Name;
                var commandUuid = executeCommand.CommandId;
                var nodeId = executeCommand.NodeId;

                MsgLogger.WriteLine($"get command status '{commandName}', node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = $"{uuid}";
                query["commandId"] = $"{commandUuid}";
                query["channelId"] = $"{executeCommand.SourceChannelId}";
                query["nodeId"] = $"{nodeId}";
                query["commandName"] = $"{commandName}";

                var url = UrlHelper.BuildUrl(Url, "api/command/status", query);

                var json = await Transporter.Get(_userIdentity, url);

                var executeCommandStatus = JsonConvert.DeserializeObject<ExecuteCommandStatus>(json);

                if (executeCommandStatus != null)
                {
                    result = executeCommandStatus;

                    MsgLogger.WriteLine($"command '{commandName}', status '{executeCommandStatus.Status}', device nodeId={nodeId}");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetCommandStatus", e);
            }

            return result;
        }

        #endregion
    }
}