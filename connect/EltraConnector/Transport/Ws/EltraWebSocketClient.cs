using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws
{
    /// <summary>
    /// EltraWebSocketClient
    /// </summary>
    public class EltraWebSocketClient : IWebSocketClient
    {
        private ClientWebSocket _socket;

        /// <summary>
        /// Socket
        /// </summary>
        protected ClientWebSocket Socket { get { return _socket ?? (_socket = new ClientWebSocket()); } }

        /// <summary>
        /// 
        /// </summary>
        public WebSocketState State => Socket.State;

        /// <summary>
        /// 
        /// </summary>
        public WebSocketCloseStatus? CloseStatus => Socket.CloseStatus;

        /// <summary>
        /// 
        /// </summary>
        public ClientWebSocketOptions Options { get => Socket.Options; }

        /// <summary>
        /// CloseAsync
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task CloseAsync(WebSocketCloseStatus status, string description, CancellationToken token)
        {
            return Socket.CloseAsync(status, description, token);
        }

        /// <summary>
        /// CloseOutputAsync
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task CloseOutputAsync(WebSocketCloseStatus status, string description, CancellationToken token)
        {
            return Socket.CloseOutputAsync(status, description, token);
        }

        /// <summary>
        /// ConnectAsync
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task ConnectAsync(Uri uri, CancellationToken token)
        {
            return Socket.ConnectAsync(uri, token);
        }

        /// <summary>
        /// ReceiveAsync
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> segment, CancellationToken token)
        {
            return Socket.ReceiveAsync(segment, token);
        }

        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="arraySegments"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task SendAsync(ArraySegment<byte> arraySegments, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token)
        {
            return Socket.SendAsync(arraySegments, messageType, endOfMessage, token);
        }
    }
}
