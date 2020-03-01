using EltraCloud.Services;
using EltraCloud.Channels.Interfaces;
using EltraCloud.Channels.Readers;
using EltraCloudContracts.Contracts.Results;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Ws;
using EltraCommon.Logger;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Processors
{
    public class SessionChannelProcessor : IChannelProcessor
    {
        #region Private fields

        private Ip2LocationService _locationService;

        #endregion

        #region Constructors

        public SessionChannelProcessor(IPAddress source, WebSocket webSocket, ISessionService sessionService, Ip2LocationService ip2LocationService)
            : base(source, webSocket, sessionService)
        {
            _locationService = ip2LocationService;

            Reader = new ChannelReader(source, webSocket);
        }

        #endregion

        #region Methods

        public override async Task<bool> ProcessMsg(WsMessage msg)
        {
            bool result = false;

            if (msg.TypeName == typeof(SessionStatusUpdate).FullName)
            {
                try
                {
                    if (await Send(new WsMessageAck()))
                    {
                        var sessionStatusUpdate = JsonConvert.DeserializeObject<SessionStatusUpdate>(msg.Data);

                        if (sessionStatusUpdate != null)
                        {
                            result = SessionService.UpdateSessionStatus(_locationService.FindAddress(Source), sessionStatusUpdate);

                            MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"send session status, result={result}");

                            result = await Send(msg.ChannelName, new RequestResult() { Result = result });
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"instance not identified, channel={msg.ChannelName}");
                        }
                    }
                }
                catch (WebSocketException e)
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"channel '{msg.ChannelName}' exception, error code={e.ErrorCode}, ws error code={e.WebSocketErrorCode}");
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - ProcessMsg", e);
                }
            }

            return result;
        }

        #endregion
    }
}
