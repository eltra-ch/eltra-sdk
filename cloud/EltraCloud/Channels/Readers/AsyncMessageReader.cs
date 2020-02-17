using EltraCloud.Channels.Events;
using EltraCloud.Channels.Interfaces;
using EltraCommon.Logger;
using EltraCommon.Threads;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Readers
{
    public class AsyncMessageReader : EltraThread
    {
        #region Private fields

        private readonly IChannelReader _reader;

        #endregion
        
        #region Constructors

        public AsyncMessageReader(IPAddress source, WebSocket webSocket)
        {
            _reader = new ChannelReader(source, webSocket);
        }

        #endregion

        #region Events

        public event EventHandler<ReadMessageEventArgs> MessageReceived;

        #endregion

        #region Events handling

        public virtual void OnMessageReceived(ReadMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        #endregion

        #region Methods

        protected override async Task Execute()
        {
            var webSocket = _reader?.WebSocket;

            if (webSocket != null)
            {
                while (webSocket.State == WebSocketState.Open && !webSocket.CloseStatus.HasValue && ShouldRun())
                {
                    var message = await _reader.Read(CancellationToken.None);

                    if (message != null)
                    {
                        OnMessageReceived(new ReadMessageEventArgs() { Message = message });
                    }
                    else
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"null channel message!");

                        break;
                    }

                    if (_reader.IsCloseRequested)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"close channel '{_reader?.ToString()}', reason = {webSocket.CloseStatus}!");

                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", CancellationToken.None);

                        break;
                    }
                }

                if (webSocket.State == WebSocketState.Open)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Execute", $"close channel '{_reader?.ToString()}', reason = {webSocket.CloseStatus}!");

                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", CancellationToken.None);
                }
            }

            RequestStop();
        }

        #endregion
    }
}
