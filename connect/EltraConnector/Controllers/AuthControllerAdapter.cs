﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraCommon.Contracts.Users;
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

        public async Task<bool> SignIn(UserData authData)
        {
            bool result = false;

            try
            {
                var path = "api/user/sign-in";

                var json = JsonConvert.SerializeObject(authData);

                var response = await Transporter.Post(Url, path, json);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - SignIn", $"Sign-in for user {authData.Login} successful!");
                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - SignIn", $"Sign-in for user {authData.Login} failed! status code = {response.StatusCode}");
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

                result = await Transporter.Get(url, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> SignUp(UserData authData)
        {
            bool result = false;

            try
            {
                var path = "api/user/sign-up";

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
