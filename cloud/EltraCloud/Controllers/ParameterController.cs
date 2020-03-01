using EltraCommon.Logger;
using EltraCloud.Services;
using EltraCloudContracts.Contracts.Parameters;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Http;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Parameter controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterController : Controller
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIp2LocationService _locationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameter controller constructor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="locationService"></param>
        /// <param name="sessionService"></param>
        public ParameterController(IHttpContextAccessor contextAccessor, IIp2LocationService locationService, ISessionService sessionService)
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
        /// Update parameter
        /// </summary>
        /// <param name="parameterUpdate">ParameterUpdate</param>
        /// <returns>bool</returns>
        [HttpPut("update")]
        public IActionResult Update([FromBody] ParameterUpdate parameterUpdate)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            var location = _sessionService.GetSessionLocation(parameterUpdate.SessionUuid);
            var address = _contextAccessor.HttpContext.Connection.RemoteIpAddress;
            var ipLocation = _locationService.FindAddress(address);
            bool result = false;

            if (ipLocation.Equals(location))
            {
                MsgLogger.WriteDebug($"{GetType().Name} - Update", $"Update Parameter Value, {parameterUpdate.Parameter.UniqueId}");

                result = _sessionService.UpdateParameterValue(parameterUpdate);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - Update", startTime, $"update parameter '{parameterUpdate.Parameter.UniqueId}', result={result}");
            }

            return Json(result);
        }

        /// <summary>
        /// Get Parameter
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="index">Parameter Index</param>
        /// <param name="subIndex">Parameter Subindex</param>
        /// <returns></returns>
        [HttpGet("get")]
        public IActionResult GetParameter(string uuid, ulong serialNumber, ushort index, byte subIndex)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();
            
            var parameterEntry = _sessionService.GetParameter(serialNumber, index, subIndex);

            if (parameterEntry != null)
            {
                var json = JsonConvert.SerializeObject(parameterEntry);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetParameter", startTime, 
                    $"get parameter index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number='0x{serialNumber:X4}' - success");

                result = Content(json, "application/json");
            }
            else
            {
                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetParameter", startTime, $"get parameter index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number='0x{serialNumber:X4}' - failed!");
            }

            return result;
        }

        /// <summary>
        /// Get Parameter Value
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="index">Parameter Index</param>
        /// <param name="subIndex">Parameter Subindex</param>
        /// <returns>ParameterValue</returns>
        [HttpGet("value")]
        public IActionResult GetParameterValue(string uuid, ulong serialNumber, ushort index, byte subIndex)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();

            var parameterValue = _sessionService.GetParameterValue(serialNumber, index, subIndex);

            if (parameterValue != null)
            {
                var json = JsonConvert.SerializeObject(parameterValue);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetParameterValue", startTime,
                    $"get parameter index=0x{index:X4}, subindex=0x{subIndex:X4} value, device serial number='0x{serialNumber:X4}' - success");

                result = Content(json, "application/json");
            }
            else
            {
                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetParameterValue", startTime, $"get parameter index=0x{index:X4}, subindex=0x{subIndex:X4} value, device serial number='0x{serialNumber:X4}' - failed!");
            }

            return result;
        }

        /// <summary>
        /// Get Parameter History
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="uniqueId">Parameter unique Id</param>
        /// <param name="from">from date/time</param>
        /// <param name="to">to date/time</param>
        /// <returns>History list</returns>
        /// <response code="204">No content</response>
        [HttpGet("history")]
        public IActionResult GetHistory(string uuid, ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();

            var history = _sessionService.GetParameterHistory(serialNumber, uniqueId, from, to);

            if (history != null && history.Count>0)
            {
                var json = JsonConvert.SerializeObject(history);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetHistory", startTime,
                    $"get parameter '{uniqueId}' history, device serial number='0x{serialNumber:X4}' - success");

                result = Content(json, "application/json");
            }
            else if(history != null && history.Count == 0)
            {
                result = NoContent();

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetHistory", startTime,
                    $"get parameter '{uniqueId}' history, device serial number='0x{serialNumber:X4}' - no content");
            }
            else
            {
                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetHistory", startTime, $"get parameter '{uniqueId}' history, device serial number='0x{serialNumber:X4}' - failed!");
            }

            return result;
        }

        /// <summary>
        /// Get Parameter History Touple
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="serialNumber">Device Id</param>
        /// <param name="uniqueId1">Parameter A unique Id</param>
        /// <param name="uniqueId2">Parameter B unique Id</param>
        /// <param name="from">from date/time</param>
        /// <param name="to">to date/time</param>
        /// <returns>History list</returns>
        /// <response code="204">No content</response>
        [HttpGet("pair-history")]
        public IActionResult GetPairHistory(string uuid, ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();

            var history = _sessionService.GetParameterPairHistory(serialNumber, uniqueId1, uniqueId2, from, to);

            if (history != null && history.Count > 0)
            {
                var json = JsonConvert.SerializeObject(history);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetPairHistory", startTime,
                    $"get parameter pair '{uniqueId1}','{uniqueId2}' history, device serial number='0x{serialNumber:X4}' - success");

                result = Content(json, "application/json");
            }
            else if (history != null && history.Count == 0)
            {
                result = NoContent();

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetPairHistory", startTime,
                    $"get parameter pair '{uniqueId1}','{uniqueId2}' history, device serial number='0x{serialNumber:X4}' - no content");
            }
            else
            {
                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetPairHistory", startTime, $"get parameter pair '{uniqueId1}', '{uniqueId2}' history, device serial number='0x{serialNumber:X4}' - failed!");
            }

            return result;
        }

        #endregion
    }
}
