using EltraCloud.Services;
using EltraCloudContracts.Contracts.Ws;
using EltraCommon.Logger;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Interfaces
{
    public class IChannelProcessor
    {
        #region Constructors

        public IChannelProcessor(IPAddress source, WebSocket webSocket, ISessionService sessionService)
        {
            Source = source;
            WebSocket = webSocket;
            SessionService = sessionService;
        }

        #endregion

        #region Properties

        public WebSocket WebSocket { get; }

        protected ISessionService SessionService { get; }

        protected IPAddress Source { get; }

        public bool IsCloseRequested
        {
            get
            {
                bool result = false;

                if (Reader != null)
                {
                    result = Reader.LastMessageType == WebSocketMessageType.Close;
                }

                return result;
            }
        }

        public IChannelReader Reader { get; set; }

        #endregion

        #region Methods

        public virtual Task<bool> ProcessMsg(WsMessage msg)
        {
            return Task.Run(()=> { return false; });
        }

        protected async Task<bool> Send<T>(string channelName, T data)
        {
            var wsResponse = new WsMessage
            {
                ChannelName = channelName,
                TypeName = data.GetType().FullName,

                Data = JsonConvert.SerializeObject(data)
            };

            return await Send(wsResponse);
        }

        public async Task<bool> Send(WsMessage msg)
        {
            bool result = false;
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

            if(await SendAsync(buffer))
            {
                if(msg is WsMessageAck)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Send", $"Send ACK response over channel '{msg.ChannelName}' to '{Source?.ToString()}' success!");

                    result = true;
                }
                else
                {
                    var ackMessage = await Reader.Read(CancellationToken.None);

                    if (ackMessage != null && ackMessage.Data == "ACK")
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Send", $"ACK received to '{Source?.ToString()}' for channel '{msg.ChannelName}' successfully!");

                        result = true;
                    }                    
                    else if(WebSocket.State == WebSocketState.Open)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Send", $"ACK message not received, channel='{msg.ChannelName}' source='{Source?.ToString()}'!");
                    }
                    else
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Send", $"ACK not received to '{Source?.ToString()}' for channel '{msg.ChannelName}', status = {WebSocket.State}");
                    }
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - Send", $"Send response over channel '{msg.ChannelName}' to '{Source?.ToString()}' failed!");
            }

            return result;
        }

        protected async Task<bool> SendAsync(byte[] buffer)
        {
            bool result = false;

            if(!WebSocket.CloseStatus.HasValue && WebSocket.State == WebSocketState.Open)
            { 
                await WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length),
                                                                 WebSocketMessageType.Text,
                                                                 true,
                                                                 CancellationToken.None);

                if(WebSocket.State == WebSocketState.Open && !WebSocket.CloseStatus.HasValue)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - SendAsync", $"SendAsync to '{Source?.ToString()}' success, bytes sent = {buffer.Length}");

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - SendAsync", $"SendAsync to '{Source?.ToString()}' failed, reason = {WebSocket.CloseStatus}!");
                }
            }
            else
            {
                MsgLogger.WriteDebug($"{GetType().Name} - SendAsync", $"SendAsync cannot send to '{Source?.ToString()}', reason = {WebSocket.CloseStatus}!");
            }

            return result;
        }

        
    #endregion
    }
}
