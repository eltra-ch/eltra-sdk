using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Users;
using Newtonsoft.Json;
using System.Threading;

namespace EltraConnector.Controllers
{
    public class AuthControllerAdapter : CloudControllerAdapter
    {
        #region Constructors

        public AuthControllerAdapter(string url) : base(url)
        {
        }

        #endregion

        #region Methods

        public async Task<bool> SignIn(UserAuthData authData)
        {
            bool result = false;

            try
            {
                var path = "api/auth/sign-in";

                var json = JsonConvert.SerializeObject(authData);

                var response = await Transporter.Post(Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
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

                var url = UrlHelper.BuildUrl(Url, "api/auth/sign-out", query);
                var cancellationTokenSource = new CancellationTokenSource();

                var statusCode = await Transporter.Get(url, cancellationTokenSource.Token);

                if (statusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> SignUp(UserAuthData authData)
        {
            bool result = false;

            try
            {
                var path = "api/auth/sign-up";

                var json = JsonConvert.SerializeObject(authData);

                var response = await Transporter.Post(Url, path, json);

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

        #endregion
    }
}
