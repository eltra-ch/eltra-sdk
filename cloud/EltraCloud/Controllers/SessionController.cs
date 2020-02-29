using EltraCommon.Logger;
using EltraCloud.Services;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Session controller
    /// </summary>
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIp2LocationService _locationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Session controller constructor
        /// </summary>
        /// <param name="contextAccessor">http context</param>
        /// <param name="locationService">location service</param>
        /// <param name="sessionService">session service</param>
        public SessionController(IHttpContextAccessor contextAccessor,
                                 IIp2LocationService locationService,
                                 ISessionService sessionService)
        {
            _locationService = locationService;
            _contextAccessor = contextAccessor;
            _sessionService = sessionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get active sessions
        /// </summary>
        /// <param name="uuid">session uuid</param>
        /// <param name="login">login name</param>
        /// <param name="password">login password</param>
        /// <returns></returns>
        [HttpGet("sessions")]
        public IActionResult GetSessions(string uuid, string login, string password)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();
            
            var sessions = _sessionService.GetSessions(login, password);

            if (sessions.Count > 0)
            {
                _sessionService.CreateSessionLink(uuid, sessions);

                var json = JsonConvert.SerializeObject(sessions);

                result = Content(json, "application/json");
            }

            MsgLogger.EndTimeMeasure($"{GetType().Name} - GetSessions", startTime, $"sessions count={sessions.Count}");

            return result;
        }

        private bool IsSessionValidForUser(string uuid, string login, string password)
        {
            bool result = false;
            var sessions = _sessionService.GetSessions(login, password);

            foreach (var session in sessions)
            {
                if (session.Uuid == uuid)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get Session by Id
        /// </summary>
        /// <param name="uuid">Session Uuid</param>
        /// <param name="login">login</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        [HttpGet("session")]
        public IActionResult GetSession(string uuid, string login, string password)
        {
            IActionResult result = NotFound();
            var startTime = MsgLogger.BeginTimeMeasure();

            if (IsSessionValidForUser(uuid, login, password))
            {
                var session = _sessionService.GetSession(uuid);
            
                if (session != null)
                {
                    string json = JsonConvert.SerializeObject(session);
                    
                    result = Content(json, "application/json");

                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetSession", startTime, $"session '{uuid}' success");
                }
                else
                {
                    MsgLogger.EndTimeMeasure($"{GetType().Name} - GetSession", startTime, $"session '{uuid}' not found!");
                }
            }
            else
            {
                result = Forbid();

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetSession", startTime, $"session '{uuid}' invalid credentials!");
            }
            
            return result;
        }

        /// <summary>
        /// Check if session exists
        /// </summary>
        /// <param name="uuid">Caller Id</param>
        /// <param name="sessionUuid">Session Id</param>
        /// <returns>RequestResult</returns>
        //GET
        [HttpGet("exists")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Exists(string uuid, string sessionUuid)
        {
            var requestResult = new RequestResult();
            var startTime = MsgLogger.BeginTimeMeasure();

            var checkResult = _sessionService.SessionExists(sessionUuid);

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Exists", startTime, $"session '{sessionUuid}' exists result={checkResult}");

            requestResult.Result = checkResult;

            var json = JsonConvert.SerializeObject(requestResult);

            return Content(json, "application/json");
        }


        /// <summary>
        /// Register session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>RequestResult</returns>
        [HttpPost ("add")]
        public IActionResult Add([FromBody]Session session)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            var requestResult = new RequestResult();
            var address = _contextAccessor.HttpContext.Connection.RemoteIpAddress;
            
            var ipLocation = _locationService.FindAddress(address);

            if (ipLocation != null)
            {
                session.IpLocation = ipLocation;
            }

            requestResult.Result = _sessionService.AddSession(session);

            MsgLogger.EndTimeMeasure($"{GetType().Name} - Add", startTime, $"session add '{session.Uuid}', result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Update Session Status
        /// </summary>
        /// <param name="statusUpdate">SessionStatusUpdate</param>
        /// <returns>RequestResult</returns>
        [HttpPut ("status")]
        public IActionResult UpdateStatus([FromBody]SessionStatusUpdate statusUpdate)
        {
            var requestResult = new RequestResult();
            var startTime = MsgLogger.BeginTimeMeasure();

            requestResult.Result = _sessionService.SetSessionStatus(statusUpdate.Id, statusUpdate.AuthData.Login, statusUpdate.Status);
            
            MsgLogger.EndTimeMeasure($"{GetType().Name} - UpdateStatus", startTime, $"update session '{statusUpdate.Id}', status={statusUpdate.Status} , result={requestResult.Result}");

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Get devices associated to session
        /// </summary>
        /// <param name="uuid">Session Id</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <returns>List{EltraDevice}</returns>
        /// <response code="403">Forbid</response>
        [HttpGet("devices")]
        public IActionResult GetSessionDevices(string uuid, string login, string password)
        {
            var startTime = MsgLogger.BeginTimeMeasure();
            IActionResult result = Forbid();

            if (IsSessionValidForUser(uuid, login, password))
            {
                var devices = _sessionService.GetSessionDevices(uuid);

                var json = JsonConvert.SerializeObject(devices);

                MsgLogger.EndTimeMeasure($"{GetType().Name} - GetSessionDevices", startTime, $"get session '{uuid}' devices count='{devices.Count}'");

                result = Content(json, "application/json");
            }

            return result;
        }

        #endregion
    }
}
