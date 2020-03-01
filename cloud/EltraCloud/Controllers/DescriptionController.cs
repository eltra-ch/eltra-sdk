using EltraCloud.Services;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCommon.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Device description api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DescriptionController : Controller
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIp2LocationService _locationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Device description constructor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="sessionService"></param>
        /// <param name="locationService"></param>
        public DescriptionController(IHttpContextAccessor contextAccessor, ISessionService sessionService, Ip2LocationService locationService)
        {
            _contextAccessor = contextAccessor;
            _sessionService = sessionService;
            _locationService = locationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Download device description
        /// </summary>
        /// <param name="hardwareVersion">Hardware version</param>
        /// <param name="softwareVersion">Software version</param>
        /// <param name="applicationNumber">Application number</param>
        /// <param name="applicationVersion">Application version</param>
        /// <returns>DeviceDescription</returns>
        /// <response code="404">Description not found</response>
        [HttpGet("download")]
        public IActionResult Download(ushort hardwareVersion, ushort softwareVersion, ushort applicationNumber, ushort applicationVersion)
        {
            IActionResult result = NotFound();

            var startTime = MsgLogger.BeginTimeMeasure();

            var deviceVersion = new DeviceVersion() { HardwareVersion= hardwareVersion , SoftwareVersion = softwareVersion, ApplicationNumber = applicationNumber, ApplicationVersion = applicationVersion};
            var deviceDescription = _sessionService.DownloadDeviceDescription(deviceVersion);

            if (deviceDescription!=null)
            {
                var json = JsonConvert.SerializeObject(deviceDescription);

                result = Content(json, "application/json");
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Download", startTime, $"download device description, version: {deviceVersion}");

            return result;
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
        /// Upload device description
        /// </summary>
        /// <param name="deviceDescription">Device description object</param>
        /// <returns>RequestResult</returns>
        [HttpPost("upload")]
        public IActionResult Upload([FromBody]DeviceDescriptionPayload deviceDescription)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            var requestResult = new RequestResult();

            if (IsRemoteSessionValid(deviceDescription.CallerUuid))
            {
                requestResult.Result = _sessionService.UploadDeviceDescription(deviceDescription);
            }
            else
            {
                requestResult.Result = false;
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Upload", startTime, $"upload device description '{deviceDescription.Version}', result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Check if device description exists
        /// </summary>
        /// <param name="uuid">Caller uuid</param>
        /// <param name="serialNumber">device id</param>
        /// <param name="hashCode">content hash code (md5)</param>
        /// <returns>RequestResult</returns>
        [HttpGet("exists")]
        public IActionResult DeviceDescriptionExists(string uuid, ulong serialNumber, string hashCode)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            var requestResult = new RequestResult();

            if (IsRemoteSessionValid(uuid))
            {
                requestResult.Result = _sessionService.DeviceDescriptionExists(serialNumber, hashCode);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - DeviceDescriptionExists", startTime, $"device description, hash='{hashCode}' exists, result={requestResult.Result}");
            }
            else
            {
                requestResult.Result = false;
                requestResult.ErrorCode = ErrorCodes.Forbid;
                requestResult.Message = "Access danied";
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        #endregion
    }
}
