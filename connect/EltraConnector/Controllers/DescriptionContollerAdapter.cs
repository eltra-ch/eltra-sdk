using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraCommon.Logger;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using EltraCommon.Helpers;
using EltraCommon.Contracts.Results;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.ToolSet;
using System.Threading;
using EltraCommon.Contracts.Users;
using EltraCommon.Transport;
using EltraCommon.Extensions;

namespace EltraConnector.Controllers
{
    internal class DescriptionControllerAdapter : CloudControllerAdapter
    {
        #region Private fields

        private UserIdentity _identity;

        #endregion

        #region Constructors

        public DescriptionControllerAdapter(UserIdentity identity, string url)
            : base(url)
        {
            _identity = identity;
        }

        #endregion

        #region Methods

        public async Task<bool> Upload(DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;

            try
            {
                MsgLogger.WriteLine($"upload device description version='{deviceDescription.Version}'");

                var postResult = await Transporter.Post(_identity, Url, "api/description/upload", deviceDescription.ToJson());

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Upload", e);
            }

            return result;
        }

        public async Task<bool> Exists(DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = deviceDescription.ChannelId;
                query["nodeId"] = $"{deviceDescription.NodeId}";
                query["hashCode"] = deviceDescription.HashCode;

                var url = UrlHelper.BuildUrl(Url, "api/description/exists", query);

                var json = await Transporter.Get(_identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    var requestResult = json.TryDeserializeObject<RequestResult>();

                    if (requestResult != null)
                    {
                        result = requestResult.Result;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Exists", e);
            }

            return result;
        }

        public async Task<DeviceDescriptionPayload> Download(string channelId, DeviceVersion deviceVersion)
        {
            DeviceDescriptionPayload result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = channelId;
                query["hardwareVersion"] = $"{deviceVersion.HardwareVersion}";
                query["softwareVersion"] = $"{deviceVersion.SoftwareVersion}";
                query["applicationNumber"] = $"{deviceVersion.ApplicationNumber}";
                query["applicationVersion"] = $"{deviceVersion.ApplicationVersion}";

                var url = UrlHelper.BuildUrl(Url, "api/description/download", query);

                var json = await Transporter.Get(_identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = json.TryDeserializeObject<DeviceDescriptionPayload>();

                    if(result!=null)
                    {
                        result.Version = deviceVersion;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Download", e);
            }

            return result;
        }

        public async Task<DeviceDescriptionIdentity> GetIdentity(string channelId, DeviceVersion deviceVersion)
        {
            DeviceDescriptionIdentity result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = channelId;
                query["hardwareVersion"] = $"{deviceVersion.HardwareVersion}";
                query["softwareVersion"] = $"{deviceVersion.SoftwareVersion}";
                query["applicationNumber"] = $"{deviceVersion.ApplicationNumber}";
                query["applicationVersion"] = $"{deviceVersion.ApplicationVersion}";

                var url = UrlHelper.BuildUrl(Url, "api/description/get-identity", query);

                var json = await Transporter.Get(_identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = json.TryDeserializeObject<DeviceDescriptionIdentity>();                    
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetIdentity", e);
            }

            return result;
        }

        internal async Task<bool> UploadPayload(DeviceToolPayload payload)
        {
            bool result = false;

            try
            {
                MsgLogger.WriteLine($"upload payload version='{payload.FileName}'");

                var postResult = await Transporter.Post(_identity, Url, "api/description/payload-upload", payload.ToJson());

                if (postResult.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Upload", e);
            }

            return result;
        }

        internal async Task<bool> PayloadExists(DeviceToolPayload payload)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = payload.ChannelId;
                query["nodeId"] = $"{payload.NodeId}";
                query["uniqueId"] = payload.Id;
                query["hashCode"] = payload.HashCode;
                query["mode"] = $"{payload.Mode}";
                query["type"] = $"{payload.Type}";

                var url = UrlHelper.BuildUrl(Url, "api/description/payload-exists", query);

                result = await Transporter.Get(_identity, url, CancellationToken.None);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - PayloadExists", e);
            }

            return result;
        }

        #endregion
    }
}
