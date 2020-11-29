using EltraConnector.Transport.Udp.Response;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpClient : EltraUdpConnector
    {
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

        #endregion
    }
}
