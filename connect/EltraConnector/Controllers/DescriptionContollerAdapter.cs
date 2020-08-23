using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraCommon.Logger;
using EltraConnector.Controllers.Base;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using EltraCommon.Helpers;
using EltraCommon.Contracts.Results;
using EltraCommon.Contracts.Devices;

namespace EltraConnector.Controllers
{
    internal class DescriptionControllerAdapter : CloudControllerAdapter
    {
        public DescriptionControllerAdapter(string url)
            : base(url)
        {

        }

        public async Task<bool> Upload(DeviceDescriptionPayload deviceDescription)
        {
            bool result = false;

            try
            {
                MsgLogger.WriteLine($"upload device description version='{deviceDescription.Version}'");

                var postResult = await Transporter.Post(Url, "api/description/upload", JsonConvert.SerializeObject(deviceDescription));

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

                var json = await Transporter.Get(url);

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

                var json = await Transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<DeviceDescriptionPayload>(json);

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
    }
}
