using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraConnector.Transport.Udp.Contracts;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
            if (_url.Contains(":"))
            {
                int separatorIndex = _url.IndexOf(":");

                Host = _url.Substring(0, separatorIndex);

                if (_url.Length > separatorIndex + 1)
                {
                    if (int.TryParse(_url.Substring(separatorIndex + 1), out var port))
                    {
                        Port = port;
                    }
                }
            }
        }

        #endregion

        #region Methods

        protected override UdpClient CreateUdpClient()
        {
            UdpClient result = new UdpClient();

            result.Connect(Host, Port);

            return result;
        }

        public async Task<int> Send(string text)
        {
            UTF8Encoding enc = new UTF8Encoding();
            var bytes = enc.GetBytes(text);

            int bytesSent = await Send(bytes, bytes.Length);

            if(bytesSent > 0)
            {
                _ = Task.Run(async () =>
                  {
                      var response = await Receive();

                      if(response != null)
                      {
                          OnMessageReceived(response);
                      }
                  });
            }

            return bytesSent;
        }

        private async Task<int> Send(UserIdentity identity, string className, string msg)
        {
            int bytesSent = -1;
            var request = new UdpRequest() { Identity = identity, TypeName = className, Data = msg };

            try
            {
                bytesSent = await Send(JsonConvert.SerializeObject(request));
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return bytesSent;
        }

        public async Task<int> Send<T>(UserIdentity identity, T obj)
        {
            int result = 0;

            try
            {
                var msg = JsonConvert.SerializeObject(obj);

                result = await Send(identity, typeof(T).FullName, msg);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        #endregion
    }
}
