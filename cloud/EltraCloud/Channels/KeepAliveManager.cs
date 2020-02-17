using EltraCloud.Channels.Interfaces;
using EltraCloudContracts.Contracts.Ws;
using EltraCommon.Threads;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace EltraCloud.Channels
{
    public class KeepAliveManager : EltraThread
    {
        #region Private fields

        private readonly IChannelProcessor _channel;

        #endregion

        #region Constructors

        public KeepAliveManager(IChannelProcessor channel)
        {
            _channel = channel;
        }

        #endregion

        #region Methods

        protected async Task<bool> SendKeepAlive()
        {
            var result = await _channel.Send(new WsMessageKeepAlive());

            return result;
        }

        protected override async Task Execute()
        {
            const long keepAliveInterval = 20000;

            var keepAliveWatch = new Stopwatch();
            bool result = true;
            const int waitIntervalInMs = 200;
            var webSocket = _channel.WebSocket;

            keepAliveWatch.Start();

            while (webSocket.State == WebSocketState.Open && !webSocket.CloseStatus.HasValue && result && ShouldRun())
            {
                if (keepAliveWatch.ElapsedMilliseconds > keepAliveInterval)
                {
                    result = await SendKeepAlive();

                    if (result)
                    {
                        keepAliveWatch.Restart();
                    }
                }
                else
                {
                    await Task.Delay(waitIntervalInMs);
                }
            }

            RequestStop();
        }

        #endregion
    }
}
