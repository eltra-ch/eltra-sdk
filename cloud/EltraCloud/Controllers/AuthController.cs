using EltraCloud.Services;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Users;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EltraCloud.Controllers
{
    /// <summary>
    /// Authentication controller 
    /// </summary>
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        #region Private fields

        private readonly IAuthService _authService;

        #endregion

        #region Constructors

        /// <summary>
        /// Auth controller constructor
        /// </summary>        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="authData">UserAuthData</param>
        /// <return>RequestResult</return>
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserAuthData authData)
        {
            var requestResult = new RequestResult();
            var authState =_authService.Register(authData.Login,authData.Name, authData.Password);

            if (authState == AuthState.UserAlreadyRegistered)
            {
                requestResult.Result = false;
                requestResult.Message = "User already registered";
            }
            else if (authState == AuthState.Success)
            {
                requestResult.Result = true;
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Sign-In 
        /// </summary>
        /// <param name="authData">UserAuthData</param>
        /// <returns>AuthRequestResult</returns>
        [HttpPost("sign-in")]
        public IActionResult SignIn([FromBody]UserAuthData authData)
        {
            var requestResult = new AuthRequestResult();
            var authState = _authService.SignIn(authData.Login, authData.Password, out var token);

            if (authState == AuthState.NoUser)
            {
                requestResult.Result = false;
                requestResult.Message = "No such user";
            }
            else if (authState == AuthState.Success)
            {
                requestResult.Token = token;
                requestResult.Result = true;
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Sign-Out 
        /// </summary>
        /// <param name="token">user token</param>
        /// <returns>RequestResult</returns>
        [HttpGet("sign-out")]
        public IActionResult SignOut(string token)
        {
            var requestResult = new RequestResult();
            var authState = _authService.SignOut(token);

            if (authState == AuthState.Success)
            {
                requestResult.Result = true;
            }
            else
            {
                requestResult.Result = false;
                requestResult.Message = "No such user";
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Check if login exists
        /// </summary>
        /// <param name="login">login name</param>
        [HttpGet("login-exists")]
        public IActionResult Exists(string login)
        {
            var requestResult = new RequestResult();
            var authState = _authService.Exists(login);

            if (authState == AuthState.Success)
            {
                requestResult.Result = true;
            }
            else if(authState == AuthState.NoUser)
            {
                requestResult.Result = false;
                requestResult.Message = "No such user";
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        /// <summary>
        /// Validate login data
        /// </summary>
        /// <param name="login">login name</param>
        /// <param name="password">password</param>
        [HttpGet("login-is-valid")]
        public IActionResult IsValid(string login, string password)
        {
            var requestResult = new RequestResult();
            var authState = _authService.IsValid(login, password);

            if (authState == AuthState.Success)
            {
                requestResult.Result = true;
            }
            else if (authState == AuthState.NoAuth)
            {
                requestResult.Result = false;
                requestResult.Message = "User not valid";
            }

            return Content(JsonConvert.SerializeObject(requestResult), "application/json");
        }

        #endregion
    }
}