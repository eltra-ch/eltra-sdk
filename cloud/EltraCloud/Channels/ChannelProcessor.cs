using EltraCloud.Services;
using EltraCloud.Channels.Factory;
using EltraCloudContracts.Contracts.Ws;
using EltraCommon.Logger;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using EltraCloud.Channels.Readers;

#pragma warning disable CS1591

namespace EltraCloud.Channels
{
    public class ChannelProcessor
    {
        #region Private fields

        private readonly ISessionService _sessionService;
        private WebSocket _webSocket;
        private IPAddress _source;

        #endregion

        #region Constructors

        public ChannelProcessor(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        #endregion

        #region Methods

        private async Task<bool> ProcessChannelMessage(WsMessage msg)
        {
            bool result = false;

            if(_webSocket!=null && msg != null)
            {
                MsgLogger.WriteLine($"Create channel='{msg.ChannelName}' processor, source = {_source.ToString()}");

                var processor = ChannelProcessorFactory.CreateProcessor(msg.ChannelName, _source, _webSocket, _sessionService);

                if (processor!=null)
                { 
                    try
                    {
                        result = await processor.ProcessMsg(msg);
                    }
                    catch(Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - ProcessChannelMessage", e);
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessChannelMessage", $"Channel='{msg.ChannelName}' processor not found!");
                }
            }

            return result;
        }

        public async Task CreateChannel(IPAddress source, WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
        {
            var reader = new ChannelReader(source, webSocket);
            string channelName = string.Empty;

            _webSocket = webSocket;
            _source = source;

            try
            {
                while(!webSocket.CloseStatus.HasValue)
                { 
                    var msg = await reader.Read(CancellationToken.None);

                    if(msg is WsMessage) 
                    {
                        channelName = msg.ChannelName;

                        MsgLogger.WriteDebug($"{GetType().Name} - CreateChannel", $"process channel '{channelName}' message from {_source?.ToString()}!");

                        if (!await ProcessChannelMessage(msg))
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - CreateChannel", $"process channel '{channelName}', source = {_source.ToString()}, state={_webSocket.State}");
                        }
                    }
                    else
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - CreateChannel", $"processing channel '{channelName}', source = {_source.ToString()}, state={_webSocket.State} finished");
                        break;
                    }
                }

                MsgLogger.WriteDebug($"{GetType().Name} - CreateChannel", $"close channel session from {_source?.ToString()}!");

                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
            }
            catch (WebSocketException e)
            {
                MsgLogger.WriteError($"{GetType().Name} - CreateChannel", $"error code={e.ErrorCode}, ws error code={e.WebSocketErrorCode}");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateChannel", e);
            }

            MsgLogger.WriteDebug($"{GetType().Name} - CreateChannel", $"close processing session from {_source?.ToString()}!");

            socketFinishedTcs.TrySetResult(null);
        }

        #endregion
    }
}
