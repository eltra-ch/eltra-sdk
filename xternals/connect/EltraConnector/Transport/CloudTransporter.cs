using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EltraCommon.Logger;

namespace EltraConnector.Transport
{
    public class CloudTransporter
    {
        #region Private fields

        private const int DefaultMaxRetryCount = 3;
        private const int DefaultMaxWaitTimeInSec = 15;
        private const int DefaultRetryTimeout = 100;

        private static HttpClient _client;
        private ClientWebSocket _webSocketClient;
        
        #endregion

        #region Constructors

        public CloudTransporter()
        {
            MaxRetryTimeout = DefaultRetryTimeout;
            MaxRetryCount = DefaultMaxRetryCount;
            MaxWaitTimeInSec = DefaultMaxWaitTimeInSec;
        }

        #endregion

        #region Properties

        public int MaxRetryCount { get; set; }
        public int MaxRetryTimeout { get; set; }
        public int MaxWaitTimeInSec { get; set; }

        private HttpClient Client => _client ?? (_client = CreateHttpClient());

        private ClientWebSocket ClientWebSocket => _webSocketClient ?? (_webSocketClient = CreateWebSocketClient());

        #endregion

        #region Methods

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(MaxWaitTimeInSec) };

            return client;
        }

        private ClientWebSocket CreateWebSocketClient()
        {
            var result = new ClientWebSocket();

            return result;
        }
        
        private async Task ExceptionHandling(int tryCount, Exception e)
        {
            if (tryCount < MaxRetryCount)
            {
                await Task.Delay(MaxRetryTimeout);
            }
            else
            {
                MsgLogger.Exception($"{GetType().Name} - ExceptionHandling", e);
            }
        }

        public async Task<TransporterResponse> Post(string url, string path, string json)
        {
            TransporterResponse result = new TransporterResponse();
            
            int tryCount = 0;

            do
            {
                try
                {
                    var message = new StringContent(json, Encoding.UTF8, "application/json");
                    var builder = new UriBuilder(url) { Path = path };

                    tryCount++;

                    MsgLogger.WriteDebug($"{GetType().Name} - Post", $"post - url ='{url}' try count = {tryCount}/{MaxRetryCount}");
                    
                    var postResult = await Client.PostAsync(builder.ToString(), message);

                    result.StatusCode = postResult.StatusCode;

                    if (postResult.IsSuccessStatusCode)
                    {
                        result.Content = await postResult.Content.ReadAsStringAsync();
                        tryCount = MaxRetryCount;
                    }
                    else if(postResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Post", $"get - url ='{url}' 401 - unautorized!");
                        tryCount = MaxRetryCount;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Post", $"post - url ='{url}' failed! response = {postResult.IsSuccessStatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    await ExceptionHandling(tryCount, e);
                    result.Exception = e;
                }
                catch (Exception e)
                {
                    await ExceptionHandling(tryCount, e);
                    result.Exception = e;
                }
            } while (tryCount < MaxRetryCount);
            
            return result;
        }

        public async Task<string> Get(string url)
        {
            string result = string.Empty;
            int tryCount = 0;

            do
            {
                try
                {
                    tryCount++;

                    MsgLogger.WriteDebug($"{GetType().Name} - Get", $"get - url ='{url}' try count = {tryCount}/{MaxRetryCount}");
                    
                    using (var response = await Client.GetAsync(url))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var streamReader = new StreamReader(stream))
                                {
                                    result = await streamReader.ReadToEndAsync();
                                    tryCount = MaxRetryCount;
                                }
                            }
                        }
                        else if(response.StatusCode == HttpStatusCode.NotFound)
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - Get", $"get - url ='{url}' not found!");
                            tryCount = MaxRetryCount;
                        }
                        else if(response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - Get", $"get - url ='{url}' 401 - unautorized!");
                            tryCount = MaxRetryCount;
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - Get", $"get - url ='{url}' failed! response = {response.IsSuccessStatusCode}");
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    await ExceptionHandling(tryCount, e);
                }
                catch (Exception e)
                {
                    await ExceptionHandling(tryCount, e);
                }
            } while (tryCount < MaxRetryCount);

            return result;
        }

        public async Task<string> Delete(string url)
        {
            string result = string.Empty;

            int tryCount = 0;

            do
            {
                try
                {
                    tryCount++;

                    MsgLogger.WriteDebug($"{GetType().Name} - Delete", $"delete - url ='{url}' try count = {tryCount}/{MaxRetryCount}");
                    
                    var deleteResult = await Client.DeleteAsync(url);

                    if (deleteResult.IsSuccessStatusCode)
                    {
                        result = await deleteResult.Content.ReadAsStringAsync();
                        tryCount = MaxRetryCount;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Delete", $"delete - url ='{url}' failed! response = {deleteResult.IsSuccessStatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    await ExceptionHandling(tryCount, e);
                }
                catch (Exception e)
                {
                    await ExceptionHandling(tryCount, e);
                }
            } while (tryCount < MaxRetryCount);

            return result;
        }

        public async Task<TransporterResponse> Put(string url, string path, string json)
        {
            var result = new TransporterResponse();
            int tryCount = 0;
            
            do
            {
                try
                {
                    var message = new StringContent(json, Encoding.UTF8, "application/json");
                    var builder = new UriBuilder(url) { Path = path };

                    tryCount++;
                    
                    var postResult = await Client.PutAsync(builder.ToString(), message);

                    result.StatusCode = postResult.StatusCode;

                    if (postResult.IsSuccessStatusCode)
                    {
                        result.Content = await postResult.Content.ReadAsStringAsync();
                        tryCount = MaxRetryCount;
                    }
                    else if(postResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Put", $"put - url ='{url}' 401 - unauthorized!");
                        tryCount = MaxRetryCount;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Put", $"put - url ='{url}' failed! response = {postResult.IsSuccessStatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    await ExceptionHandling(tryCount, e);
                }
                catch (Exception e)
                {
                    await ExceptionHandling(tryCount, e);

                    result.Exception = e;
                }
            } while (tryCount < MaxRetryCount);

            return result;
        }

        #endregion
    }
}
