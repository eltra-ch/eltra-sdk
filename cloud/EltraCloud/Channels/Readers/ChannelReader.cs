using EltraCloud.Channels.Events;
using EltraCloud.Channels.Interfaces;
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

namespace EltraCloud.Channels.Readers
{
    public class ChannelReader : IChannelReader
    {
        #region Constructors

        public ChannelReader(IPAddress source, WebSocket webSocket)
            : base(source, webSocket)
        {
        }

        #endregion

        #region Events

        event EventHandler<ChannelMessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Events handling

        public virtual void OnMessageReceived(ChannelMessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public override async Task<WsMessage> Read(CancellationToken cancellationToken)
        {
            WsMessage result = null;
            var buffer = new byte[BufferSize];
            var segment = new ArraySegment<byte>(buffer);
            var fullBuffer = new byte[BufferSize];
            int offset = 0;
            int fullBufferSize = 0;

            try
            {
                var receiveResult = await WebSocket.ReceiveAsync(segment, cancellationToken);

                LastMessageType = receiveResult.MessageType;

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Read", $"close channel '{Source?.ToString()}', reason = {WebSocket.CloseStatus}!");

                    await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", cancellationToken);
                }
                else
                {
                    while (!receiveResult.CloseStatus.HasValue)
                    {
                        if (receiveResult.EndOfMessage)
                        {
                            if (fullBufferSize > 0)
                            {
                                if (fullBuffer.Length < receiveResult.Count + offset)
                                {                                    
                                    Array.Resize(ref fullBuffer, receiveResult.Count + offset);
                                }

                                Array.Copy(buffer, 0, fullBuffer, offset, receiveResult.Count);

                                offset += receiveResult.Count;
                                fullBufferSize += receiveResult.Count;
                            }
                            else
                            {
                                if (fullBuffer.Length < receiveResult.Count)
                                {                                    
                                    Array.Resize(ref fullBuffer, receiveResult.Count);
                                }

                                Array.Copy(buffer, 0, fullBuffer, 0, receiveResult.Count);

                                fullBufferSize = receiveResult.Count;
                            }

                            var json = Encoding.UTF8.GetString(fullBuffer, 0, fullBufferSize);

                            if (!string.IsNullOrEmpty(json))
                            {
                                var msg = JsonConvert.DeserializeObject<WsMessage>(json);

                                if (msg is WsMessage)
                                {
                                    if (msg.TypeName == typeof(WsMessage).FullName)
                                    {
                                        result = JsonConvert.DeserializeObject<WsMessage>(msg.Data);
                                    }
                                    else if (msg.TypeName == typeof(WsMessageAck).FullName)
                                    {
                                        result = JsonConvert.DeserializeObject<WsMessage>(msg.Data);
                                    }
                                    else
                                    {
                                        result = msg;
                                    }

                                    OnMessageReceived(new ChannelMessageReceivedEventArgs() { ChannelMessage = msg });

                                    break;
                                }
                            }

                            buffer = new byte[BufferSize];
                            offset = 0;
                            fullBufferSize = 0;
                        }
                        else
                        {
                            if (fullBuffer.Length < receiveResult.Count + offset)
                            {
                                Array.Resize(ref fullBuffer, receiveResult.Count + offset);
                            }

                            Array.Copy(buffer, 0, fullBuffer, offset, receiveResult.Count);

                            buffer = new byte[BufferSize];

                            offset += receiveResult.Count;
                            fullBufferSize += receiveResult.Count;
                        }

                        receiveResult = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                        LastMessageType = receiveResult.MessageType;

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - Read", $"close channel '{Source?.ToString()}', reason = {WebSocket.CloseStatus}!");
                            break;
                        }
                    }

                    if (receiveResult != null && receiveResult.CloseStatus.HasValue)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Read", $"close channel session from {Source?.ToString()}!");

                        await WebSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
                    }                    
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.WriteError($"{GetType().Name} - Read", $"{e.ErrorCode}, {e.WebSocketErrorCode}");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Read", e);
            }

            return result;
        }

        #endregion
    }
}
