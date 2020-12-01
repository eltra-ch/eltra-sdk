using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
using Newtonsoft.Json;
using System.Threading;
using EltraCommon.Transport;

namespace EltraConnector.Controllers
{
    internal class AuthControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private UserIdentity _identity;

        #endregion

        #region Constructors

        public AuthControllerAdapter(string url) 
            : base(url)
        {
        }

        #endregion

        #region Methods

        public async Task<bool> SignIn(UserIdentity identity)
        {
            bool result = false;

            try
            {
                if (identity != null)
                {
                    _identity = identity;

                    var path = "api/user/sign-in";

                    var json = JsonConvert.SerializeObject(identity);

                    var response = await Transporter.Post(identity, Url, path, json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - SignIn", $"Sign-in for user {identity.Login} successful!");
                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SignIn", $"Sign-in for user {identity.Login} failed! status code = {response.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignIn", e);
            }

            return result;
        }

        public async Task<bool> SignOut()
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var url = UrlHelper.BuildUrl(Url, "api/user/sign-out", query);
                var cancellationTokenSource = new CancellationTokenSource();

                result = await Transporter.Get(_identity, url, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> SignOff()
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var url = UrlHelper.BuildUrl(Url, "api/user/remove", query);
                var cancellationTokenSource = new CancellationTokenSource();

                result = await Transporter.Get(_identity, url, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOff", e);
            }

            return result;
        }

        public async Task<bool> SignUp(UserIdentity identity)
        {
            bool result = false;

            try
            {
                var path = "api/user/sign-up";

                var json = JsonConvert.SerializeObject(identity);

                var response = await Transporter.Post(identity, Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignUp", e);
            }

            return result;
        }

        internal async Task<UserIdentity> CreateAlias(string role)
        {
            UserIdentity result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["role"] = role;

                var url = UrlHelper.BuildUrl(Url, "api/user/create-alias", query);
                
                var json = await Transporter.Get(_identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<UserIdentity>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateAlias", e);
            }

            return result;
        }

        internal async Task<bool> CreateAlias(UserIdentity identity)
        {
            bool result = false;

            try
            {
                var path = "api/user/create-alias";

                var json = JsonConvert.SerializeObject(identity);

                var response = await Transporter.Post(_identity, Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateAlias", e);
            }

            return result;
        }

        #endregion
    }
}
