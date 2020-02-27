﻿using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCommon.Logger;
using EltraConnector.Controllers.Base;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using EltraCommon.Helpers;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Devices;

namespace EltraConnector.Controllers
{
    public class DescriptionControllerAdapter : CloudControllerAdapter
    {
        public DescriptionControllerAdapter(string url)
            : base(url)
        {

        }

        public async Task<bool> Upload(DeviceDescription deviceDescription)
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

        public async Task<bool> Exists(DeviceDescription deviceDescription)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["serialNumber"] = $"{deviceDescription.SerialNumber}";
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

        public async Task<DeviceDescription> Download(DeviceVersion deviceVersion)
        {
            DeviceDescription result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["hardwareVersion"] = $"{deviceVersion.HardwareVersion}";
                query["softwareVersion"] = $"{deviceVersion.SoftwareVersion}";
                query["applicationNumber"] = $"{deviceVersion.ApplicationNumber}";
                query["applicationVersion"] = $"{deviceVersion.ApplicationVersion}";

                var url = UrlHelper.BuildUrl(Url, "api/description/download", query);

                var json = await Transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<DeviceDescription>(json);

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
