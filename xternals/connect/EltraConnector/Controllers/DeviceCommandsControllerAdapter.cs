using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraConnector.Extensions;

using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Logger;
using Newtonsoft.Json;
using EltraCommon.Helpers;
using EltraCloudContracts.Contracts.Devices;

namespace EltraConnector.Controllers
{
    public class DeviceCommandsControllerAdapter : CloudSessionControllerAdapter
    {
        #region Constructors

        public DeviceCommandsControllerAdapter(string url, Session session)
            : base(url, session)
        {
        }

        #endregion
        
        #region Methods

        public async Task<List<DeviceCommand>> GetDeviceCommands(EltraDevice device)
        {
            List<DeviceCommand> result = null;

            if (device != null)
            {
                MsgLogger.WriteLine($"get device='{device.Name}', serial number=0x{device.Identification.SerialNumber:X} commands");

                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["uuid"] = Session.Uuid;
                    query["serialNumber"] = $"{device.Identification.SerialNumber}";

                    var url = UrlHelper.BuildUrl(Url, "api/command/commands", query);

                    var json = await Transporter.Get(url);

                    var commandSet = JsonConvert.DeserializeObject<DeviceCommandSet>(json);

                    AssignDeviceToCommand(device, commandSet);

                    result = commandSet.Commands;
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetDeviceCommands", e);
                }
            }

            return result;
        }

        public async Task<DeviceCommand> GetDeviceCommand(EltraDevice device, string commandName)
        {
            DeviceCommand result = null;

            if (device != null)
            {
                MsgLogger.WriteLine($"get command '{commandName}' from device='{device.Name}', serial number=0x{device.Identification.SerialNumber:X}");

                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["serialNumber"] = $"{device.Identification.SerialNumber}";
                    query["commandName"] = $"{commandName}";

                    var url = UrlHelper.BuildUrl(Url, "api/command/command", query);

                    var json = await Transporter.Get(url);

                    var command = JsonConvert.DeserializeObject<DeviceCommand>(json);

                    if (command != null)
                    {
                        command.Device = device;

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
                MsgLogger.WriteLine($"push command='{execCommand.Command.Name}' to device='{execCommand.Command.Device.Name}':0x{execCommand.SerialNumber:X}");

                var postResult = await Transporter.Post(Url, "api/command/push", execCommand.ToJson());

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
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
            var deviceIdent = command.Device?.Identification;

            if (deviceIdent != null)
            {
                var execCommand = new ExecuteCommand { Command = command, SerialNumber = deviceIdent.SerialNumber, SessionUuid = agentUuid };

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
                MsgLogger.WriteLine($"set command='{status.CommandName}' status='{status.Status}' for device with serial number=0x{status.SerialNumber:X}");

                var postResult = await Transporter.Post(Url, "api/command/status", JsonConvert.SerializeObject(status));

                if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
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
            var execCommandStatus = new ExecuteCommandStatus(executeCommand) { Status = status };

            return await SetCommandStatus(execCommandStatus);
        }

        public async Task<List<ExecuteCommand>> PopCommands(EltraDevice device, ExecCommandStatus status)
        {
            var result = new List<ExecuteCommand>();

            if (device != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["uuid"] = Session.Uuid;
                    query["serialNumber"] = $"{device.Identification.SerialNumber}";
                    query["status"] = $"{status}";
                    
                    var url = UrlHelper.BuildUrl(Url, "api/command/pops", query);

                    var json = await Transporter.Get(url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var executeCommands = JsonConvert.DeserializeObject<List<ExecuteCommand>>(json);

                        foreach (var executeCommand in executeCommands)
                        {
                            if (executeCommand?.Command != null)
                            {
                                executeCommand.Command.Device = device;
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

        public async Task<ExecuteCommand> PopCommand(string commandUuid, EltraDevice device, ExecCommandStatus status)
        {
            ExecuteCommand result = null;

            if (device != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query["uuid"] = Session.Uuid;
                    query["commandUuid"] = $"{commandUuid}";
                    query["serialNumber"] = $"{device.Identification.SerialNumber}";
                    query["status"] = $"{status}";
                    
                    var url = UrlHelper.BuildUrl(Url, "api/command/pop", query);

                    var json = await Transporter.Get(url);

                    result = JsonConvert.DeserializeObject<ExecuteCommand>(json);

                    if (result?.Command != null)
                    {
                        result.Command.Device = device;
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
                var commandUuid = executeCommand.CommandUuid;
                var serialNumber = executeCommand.SerialNumber;
                var sessionUuid = executeCommand.SessionUuid;

                MsgLogger.WriteLine($"get command status '{commandName}', device serial number=0x{serialNumber:X}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = $"{uuid}";
                query["commandUuid"] = $"{commandUuid}";
                query["sessionUuid"] = $"{sessionUuid}";
                query["serialNumber"] = $"{serialNumber}";
                query["commandName"] = $"{commandName}";

                var url = UrlHelper.BuildUrl(Url, "api/command/status", query);

                var json = await Transporter.Get(url);

                var executeCommandStatus = JsonConvert.DeserializeObject<ExecuteCommandStatus>(json);

                if (executeCommandStatus != null)
                {
                    result = executeCommandStatus;

                    MsgLogger.WriteLine($"command '{commandName}', status '{executeCommandStatus.Status}', device serial number=0x{serialNumber:X}");
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