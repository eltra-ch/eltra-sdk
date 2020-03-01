using EltraCommon.Logger;
using EltraCloud.Services;
using EltraCloudContracts.Contracts.CommandSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Command controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : Controller
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIp2LocationService _locationService;

        #endregion

        #region Constructors

        /// <summary>
        /// CommandController constructor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="sessionService"></param>
        /// <param name="locationService"></param>
        public CommandController(IHttpContextAccessor contextAccessor, ISessionService sessionService, Ip2LocationService locationService)
        {
            _contextAccessor = contextAccessor;
            _sessionService = sessionService;
            _locationService = locationService;
        }

        #endregion

        #region Methods

        private bool IsRemoteSessionValid(string uuid)
        {
            bool result = false;
            var sessionLocation = _sessionService.GetSessionLocation(uuid);
            var address = _contextAccessor.HttpContext.Connection.RemoteIpAddress;
            var ipLocation = _locationService.FindAddress(address);

            if (sessionLocation != null && ipLocation != null && ipLocation.Equals(sessionLocation))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Get all commands supported by device
        /// </summary>
        /// <param name="uuid">session uuid</param>
        /// <param name="serialNumber">device identification</param>
        [ProducesResponseType(404, Type = typeof(void))]
        [HttpGet("commands")]
        public IActionResult GetDeviceCommands(string uuid, ulong serialNumber)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(uuid))
            {
                var commandSet = _sessionService.GetDeviceCommands(serialNumber);

                if (commandSet != null)
                {
                    var json = JsonConvert.SerializeObject(commandSet);

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetDeviceCommands", startTime, $"get commands, device serial number='0x{serialNumber:X4}', commands count={commandSet.Commands.Count} - success");

                    result = Content(json, "application/json");
                }
                else
                {
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetDeviceCommands", startTime, $"get commands, device serial number='0x{serialNumber:X4}' - failed!");
                }
            }
            else
            {
                result = Forbid();
            }

            return result;
        }

        /// <summary>
        /// Get device command 
        /// </summary>
        /// <param name="uuid">caller uuid</param>
        /// <param name="serialNumber">device identification</param>
        /// <param name="commandName">command name</param>
        /// <returns>DeviceCommand</returns>
        [HttpGet("command")]
        [ProducesResponseType(404, Type = typeof(void))]
        public IActionResult GetDeviceCommand(string uuid, ulong serialNumber, string commandName)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(uuid))
            {
                var command = _sessionService.GetDeviceCommand(serialNumber, commandName);

                if (command != null)
                {
                    var json = JsonConvert.SerializeObject(command);

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetDeviceCommand", startTime, $"get command '{commandName}', device serial number='0x{serialNumber:X4}'");

                    result = Content(json, "application/json");
                }
                else
                {
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetDeviceCommand", startTime, $"get command '{commandName}', device serial number='0x{serialNumber:X4}' failed!");
                }
            }
            else
            {
                result = Forbid();
            }

            return result;
        }

        /// <summary>
        /// Push command
        /// </summary>
        /// <param name="executeCommand">ExecuteCommand</param>        
        /// <returns>HTTP Status code</returns>
        /// <response code="200">Success</response>
        /// <response code="422">Failure</response>
        [HttpPost("push")]
        public IActionResult PushCommand([FromBody]ExecuteCommand executeCommand)
        {
            IActionResult result = Forbid();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(executeCommand.SessionUuid))
            {
                if (_sessionService.PushCommand(executeCommand))
                {
                    result = Ok();
                }
                else
                {
                    result = UnprocessableEntity();
                }
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - PushCommand", startTime, $"set command '{executeCommand.Command.Name}', result={result}");

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandStatus">ExecuteCommandStatus</param>
        /// <returns>HTTP Status code</returns>
        /// <response code="200">Success</response>
        /// <response code="422">Failure</response>
        [HttpPost("status")]
        public IActionResult SetStatus([FromBody]ExecuteCommandStatus commandStatus)
        {
            IActionResult result;

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(commandStatus.SessionUuid))
            {
                if (_sessionService.SetCommandStatus(commandStatus))
                {
                    result = Ok();
                }
                else
                {
                    result = UnprocessableEntity();
                }
            }
            else
            {
                result = Forbid();
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - SetStatus", startTime, $"set command '{commandStatus.CommandName}' status={commandStatus.Status} , result={result}");

            return result;
        }

        /// <summary>
        /// Get Command Status
        /// </summary>
        /// <param name="uuid">Caller session Id</param>
        /// <param name="commandUuid">Command Id</param>
        /// <param name="sessionUuid">Session Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="commandName">Command name</param>
        /// <returns>ExecuteCommandStatus</returns>
        [HttpGet("status")]
        public IActionResult GetStatus(string uuid, string commandUuid, string sessionUuid, ulong serialNumber, string commandName)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(uuid))
            {
                var execCommandStatus = _sessionService.GetCommandStatus(commandUuid, sessionUuid, serialNumber, commandName);

                if (execCommandStatus != null)
                {
                    var json = JsonConvert.SerializeObject(execCommandStatus);

                    result = Content(json, "application/json");

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetStatus", startTime,
                        $"get command '{commandName}', status '{execCommandStatus.Status}'  - device serial number='0x{serialNumber:X4}'");
                }
                else
                {
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetStatus", startTime,
                        $"no command '{commandName}' status found! - device serial number='0x{serialNumber:X4}'");
                }
            }
            else
            {
                result = Forbid();
            }
            
            return result;
        }

        /// <summary>
        /// Retrieve Command
        /// </summary>
        /// <param name="uuid">Session Id</param>
        /// <param name="commandUuid">Command Uuid</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="status">ExecCommand Status</param>
        /// <returns>ExecuteCommand</returns>
        [HttpGet("pop")]
        public IActionResult PopCommand(string uuid, string commandUuid, ulong serialNumber, ExecCommandStatus status)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(uuid))
            {
                var executeCommand = _sessionService.PopCommand(commandUuid, serialNumber, status);

                if (executeCommand != null)
                {
                    var json = JsonConvert.SerializeObject(executeCommand);

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - PopCommand", startTime, $"next command, name='{executeCommand.Command.Name}' status={status} - device serial number='0x{serialNumber:X4}'");

                    result = Content(json, "application/json");
                }
            }
            else
            {
                result = Forbid();
            }
            
            return result;
        }

        /// <summary>
        /// Retrieve Commands
        /// </summary>
        /// <param name="uuid">Session Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="status">ExecCommandStatus</param>
        /// <returns>List of ExecuteCommand</returns>
        [HttpGet("pops")]
        public IActionResult PopCommands(string uuid, ulong serialNumber, ExecCommandStatus status)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(uuid))
            {
                var executeCommands = _sessionService.PopCommands(serialNumber, status);

                if (executeCommands != null)
                {
                    var json = JsonConvert.SerializeObject(executeCommands);

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - PopCommands", startTime, $"next commands, count='{executeCommands.Count}' status={status} - device serial number='0x{serialNumber:X4}'");

                    result = Content(json, "application/json");
                }
            }
            else
            {
                result = Forbid();
            }
            
            return result;
        }

        #endregion
    }
}