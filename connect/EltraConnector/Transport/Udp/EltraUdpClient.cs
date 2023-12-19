using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.Transport.Udp.Contracts;
using System.Text.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using EltraCommon.Extensions;
using EltraCommon.Helpers;
using EltraConnector.Extensions;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpClient : EltraUdpConnector
    {
        #region Private fields

        private string _url;

        #endregion

        #region Properties

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnUrlChanged();
            }
        }

        #endregion

        #region Callbacks

        private void OnUrlChanged()
        {
            var url = _url.Replace("http://", "");
            
            url = url.Replace("https://", "");

            if (url.Contains(":"))
            {
                int separatorIndex = url.IndexOf(":");

                Host = url.Substring(0, separatorIndex);

                if (url.Length > separatorIndex + 1 && int.TryParse(url.Substring(separatorIndex + 1), out var port))
                {
                    Port = port;
                }
            }
        }

        #endregion

        #region Methods

        protected override UdpClientWrapper CreateUdpClient()
        {
            var result = new UdpClientWrapper();

            return result;
        }

        public bool Connect()
        {
            bool result = false;

            try
            {
                UdpClient.Connect(Host, Port);

                result = true;
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Connect", e);
            }

            return result;
        }

        public bool Disconnect()
        {
            bool result = false;

            try
            {
                Abort();

                UdpClient.Close();

                result = true;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Disconnect", e);
            }

            return result;
        }

        public async Task<int> Send(string text)
        {
            UTF8Encoding enc = new UTF8Encoding();
            var bytes = enc.GetBytes(text);
            int bytesSent = 0;

            if (!IsCanceled)
            {
                bytesSent = await Send(bytes, bytes.Length);

                if (bytesSent > 0)
                {
                    _ = Task.Run(async () =>
                      {
                          if (!IsCanceled)
                          {
                              var response = await Receive();

                              if (response != null)
                              {
                                  OnMessageReceived(response);
                              }
                          }
                      });
                }
            }

            return bytesSent;
        }

        public async Task<int> Send(UserIdentity identity, string className, string msg)
        {
            int bytesSent = -1;
            var data = msg.ToBase64();
            
            identity.Password = CryptHelpers.ToSha256(identity.Password);

            var request = new UdpRequest() { Identity = identity.HashPassword(), TypeName = className, Data = data, Checksum = CryptHelpers.ToMD5(data) };

            if (!IsCanceled)
            {
                try
                {
                    bytesSent = await Send(JsonSerializer.Serialize(request));
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Send", e);
                }
            }

            return bytesSent;
        }

        public async Task<int> Send<T>(UserIdentity identity, T obj)
        {
            int result = 0;

            if (!IsCanceled)
            {
                try
                {
                    var msg = JsonSerializer.Serialize(obj);

                    result = await Send(identity, typeof(T).FullName, msg);
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Send", e);
                }
            }

            return result;
        }

        #endregion
    }
}
