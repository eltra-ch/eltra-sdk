using EltraCommon.Logger;
using EltraCloud.Services;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Sessions;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Device Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : Controller
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIp2LocationService _locationService;

        #endregion

        #region Constructors

        /// <summary>
        /// DeviceController Constructor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="sessionService"></param>
        /// <param name="locationService"></param>
        public DeviceController(IHttpContextAccessor contextAccessor, ISessionService sessionService, Ip2LocationService locationService)
        {
            _contextAccessor = contextAccessor;
            _sessionService = sessionService;
            _locationService = locationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check if device exists
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="sessionUuid">Session id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <returns></returns>
        [HttpGet("exists")]
        public IActionResult DeviceExists(string uuid, string sessionUuid, string serialNumber)
        {
            bool result = false;
            var startTime = MsgLogger.BeginTimeMeasure();
            
            if (IsRemoteSessionValid(uuid))
            {
                if (ulong.TryParse(serialNumber, out var sn))
                {
                    result = _sessionService.DeviceExists(sessionUuid, sn);
                }
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - DeviceExists", startTime, $"device serial number='0x{serialNumber:X4}' exists, result={result}");

            return Json(result);
        }
        
        /// <summary>
        /// Register device
        /// </summary>
        /// <param name="sessionDevice">SessionDevice</param>
        /// <returns>true/false</returns>
        [HttpPost("add")]
        public IActionResult RegisterDevice([FromBody]SessionDevice sessionDevice)
        {
            var startTime = MsgLogger.BeginTimeMeasure();

            bool result = _sessionService.RegisterDevice(_contextAccessor.HttpContext.Connection, sessionDevice);

            MsgLogger.EndTimeMeasure($"{GetType().Name} - RegisterDevice", startTime, $"register device, session='{sessionDevice.SessionUuid}' serial number='{sessionDevice.Device.Identification.SerialNumber}' exists, result={result}");

            return Json(result);
        }

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
        /// Remove device
        /// </summary>
        /// <param name="serialNumber">Device id</param>
        /// <param name="uuid">Caller id</param>
        /// <returns></returns>
        [HttpDelete("remove/{uuid}/{serialNumber}")]
        public IActionResult RemoveDevice(string uuid, string serialNumber)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            bool result = false;
            
            if (IsRemoteSessionValid(uuid))
            {
                if (ulong.TryParse(serialNumber, out var sn))
                {
                    result = _sessionService.RemoveDevice(uuid, sn);
                }
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - RemoveDevice", startTime, $"remove device serial number='{serialNumber}', result={result}");
            
            return Json(result);
        }

        /// <summary>
        /// Lock device
        /// </summary>
        /// <param name="deviceLock">DeviceLock</param>
        /// <returns>RequestResult</returns>
        [HttpPost("lock")]
        public IActionResult Lock([FromBody]DeviceLock deviceLock)
        {
            var requestResult = new RequestResult();
            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(deviceLock.AgentUuid))
            {
                if (_sessionService.CanLockDevice(deviceLock.AgentUuid, deviceLock.SerialNumber))
                {
                    if (_sessionService.LockDevice(deviceLock.AgentUuid, deviceLock.SerialNumber))
                    {
                        requestResult.Message = "Device locked";
                    }
                    else
                    {
                        requestResult.ErrorCode = ErrorCodes.DeviceLockFailed;
                        requestResult.Message = "Device already locked";
                        requestResult.Result = false;
                    }
                }
                else
                {
                    requestResult.ErrorCode = ErrorCodes.DeviceLocked;
                    requestResult.Message = "Device already locked";
                    requestResult.Result = false;
                }
            }
            else
            {
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
                requestResult.Result = false;
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Lock", startTime, $"lock device '0x{deviceLock.SerialNumber:X4}' by agent={deviceLock.AgentUuid} , result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Validate if user can lock device
        /// </summary>
        /// <param name="deviceLock"></param>
        /// <returns></returns>
        [HttpPost("can-agent-lock")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult CanLock([FromBody]DeviceLock deviceLock)
        {
            var requestResult = new RequestResult();
            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(deviceLock.AgentUuid))
            {
                if (_sessionService.CanLockDevice(deviceLock.AgentUuid, deviceLock.SerialNumber))
                {
                    requestResult.Result = true;
                }
                else
                {
                    requestResult.ErrorCode = ErrorCodes.DeviceLocked;
                    requestResult.Message = "Device already locked";
                    requestResult.Result = false;
                }
            }
            else
            {
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
                requestResult.Result = false;
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - CanLock", startTime, $"lock device '0x{deviceLock.SerialNumber:X4}' by agent={deviceLock.AgentUuid} , result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Is device locked by specific user
        /// </summary>
        /// <param name="deviceLock"></param>
        /// <returns></returns>
        [HttpPost("is-locked-by-agent")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult IsLocked([FromBody]DeviceLock deviceLock)
        {
            var requestResult = new RequestResult();
            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsRemoteSessionValid(deviceLock.AgentUuid))
            {
                if (_sessionService.IsDeviceLockedByAgent(deviceLock.AgentUuid, deviceLock.SerialNumber))
                {
                    requestResult.Message = "Device locked";
                }
                else
                {
                    requestResult.ErrorCode = ErrorCodes.DeviceLocked;
                    requestResult.Message = "Device locked by another agent";
                    requestResult.Result = false;
                }
            }
            else
            {
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
                requestResult.Result = false;
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - IsLocked", startTime, $"lock device '0x{deviceLock.SerialNumber:X4}' by agent={deviceLock.AgentUuid} , result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Unlock device
        /// </summary>
        /// <param name="deviceLock"></param>
        /// <returns></returns>
        [HttpPost("unlock")]
        public IActionResult Unlock([FromBody]DeviceLock deviceLock)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            var requestResult = new RequestResult();

            if (IsRemoteSessionValid(deviceLock.AgentUuid))
            {
                if (_sessionService.IsDeviceLockedByAgent(deviceLock.AgentUuid, deviceLock.SerialNumber))
                {
                    if (!_sessionService.UnlockDevice(deviceLock.AgentUuid, deviceLock.SerialNumber))
                    {
                        requestResult.ErrorCode = ErrorCodes.DeviceUnlockFailed;
                        requestResult.Message = "Device unlock failed!";
                        requestResult.Result = false;
                    }
                }
                else
                {
                    requestResult.ErrorCode = ErrorCodes.DeviceNotLocked;
                    requestResult.Message = "Device is not locked";
                    requestResult.Result = false;
                }
            }
            else
            {
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
                requestResult.Result = false;
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Unlock", startTime, $"unlock device '0x{deviceLock.SerialNumber:X4}' by agent={deviceLock.AgentUuid} , result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        #endregion
    }
}