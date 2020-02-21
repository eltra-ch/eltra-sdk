using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using Newtonsoft.Json;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    public class DeviceDescriptionFile
    {
        #region Private fields

        private string _content;

        #endregion

        #region Constructors

        public DeviceDescriptionFile(EltraDevice device)
        {
            Device = device;
        }

        #endregion

        #region Properties

        public string Url { get; set; }

        public string SourceFile { get; set; }
        
        public string Content 
        {
            get => _content;
            set
            {
                _content = value;

                OnContentChanged();
            }
        }

        public string ProductName { get; set; }

        public EltraDevice Device { get; }
        
        public string HashCode
        {
            get
            {
                return CryptHelpers.ToMD5(Content);
            }
        }

        public string Encoding
        {
            get
            {
                return "MD5";
            }
        }

        #endregion

        #region Events

        public event EventHandler<DeviceDescriptionEventArgs> StateChanged;

        #endregion

        #region Events handling

        protected virtual void OnDeviceDescriptionStateChanged(DeviceDescriptionEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        private void OnContentChanged()
        {
            ReadProductName();
            
            if(!ReadDeviceVersion())
            {
                MsgLogger.WriteError($"{GetType().Name} - OnContentChanged", "read device version failed!");
            }

            if (!ReadDeviceTools())
            {
                MsgLogger.WriteError($"{GetType().Name} - OnContentChanged", "read device tools failed!");
            }
        }

        #endregion

        #region Methods

        private async Task<string> Get(string url)
        {
            string result = string.Empty;
            
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var streamReader = new StreamReader(stream))
                                {
                                    result = await streamReader.ReadToEndAsync();
                                }
                            }
                        }                        
                    }
                }
            }
            catch (HttpRequestException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Get", e.InnerException);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Get", e);
            }
            
            return result;
        }

        public async Task<bool> Exists(DeviceDescription deviceDescription)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["hashCode"] = deviceDescription.HashCode;

                var url = UrlHelper.BuildUrl(Url, "api/description/exists", query);

                var json = await Get(url);

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

        private async Task<DeviceDescription> Download(DeviceVersion deviceVersion)
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

                var json = await Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<DeviceDescription>(json);

                    if (result != null)
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
        
        public virtual async Task Read()
        {
            if(File.Exists(SourceFile))
            {
                if (string.IsNullOrEmpty(Content))
                {
                    ReadFile();
                }
            }
            else
            {
                await DownloadFile();
            }
        }

        private async Task DownloadFile()
        {
            var deviceDescription = await Download(Device.Version);

            if (deviceDescription != null)
            {
                Content = deviceDescription.PlainContent;

                if (!string.IsNullOrEmpty(Content))
                {
                    OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Read });
                }
            }
        }

        protected virtual void ReadProductName()
        {
        }

        protected virtual bool ReadDeviceVersion()
        {
            return false;
        }

        protected virtual bool ReadDeviceTools()
        {
            return true; //optional
        }

        private bool ReadFile()
        {
            bool result = false;

            try
            {
                if (File.Exists(SourceFile))
                {
                    Content = File.ReadAllText(SourceFile);

                    if (!string.IsNullOrEmpty(Content))
                    {
                        result = true;

                        OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Read });
                    }
                }
                else if(!string.IsNullOrEmpty(SourceFile))
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - ReadFile", $"device description file '{SourceFile}'");
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - ReadFile", $"device description file not spefified!");
                }
            }
            catch (Exception e)
            {
                OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed, Exception = e });
            }

            return result;
        }

        #endregion
    }
}
