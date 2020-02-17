using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraConnector.Transport;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Users;
using Newtonsoft.Json;

namespace EltraConnector.Controllers
{
    public class AuthControllerAdapter : CloudControllerAdapter
    {
        private readonly CloudTransporter _transporter;

        public AuthControllerAdapter(string url) : base(url)
        {
            _transporter = new CloudTransporter();
        }

        public async Task<bool> LoginExists(string login)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["login"] = login;

                var url = UrlHelper.BuildUrl(Url, "api/auth/login-exists", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(json);

                    if (requestResult != null)
                    {
                        result = requestResult.Result;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - LoginExists", e);
            }

            return result;
        }

        public async Task<bool> IsValid(string login, string password)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["login"] = login;
                query["password"] = password;

                var url = UrlHelper.BuildUrl(Url, "api/auth/login-is-valid", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(json);

                    if (requestResult != null)
                    {
                        result = requestResult.Result;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - IsValid", e);
            }

            return result;
        }
        
        public async Task<string> SignIn(UserAuthData authData)
        {
            string result = string.Empty;

            try
            {
                var path = "api/auth/sign-in";

                var json = JsonConvert.SerializeObject(authData);

                var response = await _transporter.Post(Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<AuthRequestResult>(response.Content);

                    result = requestResult.Token;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignIn", e);
            }

            return result;
        }

        public async Task<bool> SignOut(string token)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["token"] = token;

                var url = UrlHelper.BuildUrl(Url, "api/auth/sign-out", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(json);

                    if (requestResult != null)
                    {
                        result = requestResult.Result;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> Register(UserAuthData authData)
        {
            bool result = false;

            try
            {
                var path = "api/auth/register";

                var json = JsonConvert.SerializeObject(authData);

                var response = await _transporter.Post(Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var requestResult = JsonConvert.DeserializeObject<RequestResult>(response.Content);

                    result = requestResult.Result;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Register", e);
            }

            return result;
        }
    }
}
