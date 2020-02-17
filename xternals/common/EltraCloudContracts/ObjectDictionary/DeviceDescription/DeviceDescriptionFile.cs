using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraResources;
using Newtonsoft.Json;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    public class DeviceDescriptionFile
    {
        #region Private fields

        private List<string> _urls;
        private string _content;

        #endregion

        #region Constructors

        public DeviceDescriptionFile(EltraDevice device)
        {
            Url = "http://localhost";
            FileExtension = "xdd";

            Device = device;
        }

        #endregion

        #region Properties

        public string Url { get; set; }

        public List<string> Urls
        {
            get => _urls ?? (_urls = new List<string>());
            set => _urls = value;
        }

        public string FileExtension { get; set; }
        
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
        }

        #endregion

        #region Methods

        public async Task<string> Get(string url)
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
            catch (HttpRequestException)
            {
            }
            catch (Exception)
            {
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
                MsgLogger.Exception("DeviceDescriptionFile - Exists", e);
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
                MsgLogger.Exception("DeviceDescriptionFile - Download", e);
            }

            return result;
        }

        public void AddUrl(string url)
        {
            Urls.Add(url);
        }

        public virtual async Task Read()
        {
            await DownloadFile();

            if(string.IsNullOrEmpty(Content))
            {
                await ReadFileFromResources();
            }

            if (string.IsNullOrEmpty(Content))
            {
                await ReadFile();
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

        private string GetDeviceDescriptionFileName()
        {
            var version = Device.Version;

            var result =
                $"{Device.Name}_{version.SoftwareVersion:X4}h_{version.HardwareVersion:X4}h_{version.ApplicationNumber:X4}h_{version.ApplicationVersion:X4}h.{FileExtension}";

            return result;
        }

        private string GetDeviceDescriptionFileUri(string url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            var result = $"{url}{GetDeviceDescriptionFileName()}";

            return result;
        }

        protected async Task<string> GetContentFromResources()
        {
            const string resourcePath = "devices.fw";
            string fileName = GetDeviceDescriptionFileName();
            var resource = new EltraResource();

            var result = await resource.GetFileContent(resourcePath, fileName);

            return result;
        }

        private string DownloadContentAsync()
        {
            string result = string.Empty;

            const double maxTimeoutInSec = 30;
            var tasks = new List<Task>();

            foreach (var url in Urls)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        using (HttpClient client = new HttpClient {Timeout = TimeSpan.FromSeconds(maxTimeoutInSec)})
                        {
                            client.DefaultRequestHeaders.Accept.Clear();

                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/xdd"));

                            client.DefaultRequestHeaders.Add("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36");
                            var uri = GetDeviceDescriptionFileUri(url);

                            var response = await client.GetAsync(uri);

                            if (response.IsSuccessStatusCode)
                            {
                                string content = await response.Content.ReadAsStringAsync();
                                
                                if (!string.IsNullOrEmpty(content))
                                {
                                    Url = url;

                                    Content = content;

                                    OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs
                                        {DeviceDescription = this, State = DeviceDescriptionState.Read});
                                }
                            }
                            else
                            {
                                OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed});
                            }
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed, Exception = e });
                    }
                    catch (Exception e)
                    {
                        OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed, Exception = e });
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAny(tasks.ToArray());

            return result;
        }

        private async Task ReadFileFromResources()
        {
            try
            {
                Content = await GetContentFromResources();

                if (!string.IsNullOrEmpty(Content))
                {
                    OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Read });
                }
            }
            catch (Exception e)
            {
                OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed, Exception = e });
            }
        }

        private async Task ReadFile()
        {
            try
            {
                Content = await GetContentFromResources();

                if(string.IsNullOrEmpty(Content))
                {
                    Content = DownloadContentAsync();
                }
                else
                {
                    OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Read });
                }
            }
            catch (Exception e)
            {
                OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Failed, Exception = e });
            }
        }
        
        #endregion
    }
}
