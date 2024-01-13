using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws
{
    /// <summary>
    /// IWebSocketClient
    /// </summary>
    public interface IWebSocketClient
    {
        /// <summary>
        /// State
        /// </summary>
        WebSocketState State { get; }
        /// <summary>
        /// CloseStatus
        /// </summary>
        WebSocketCloseStatus? CloseStatus { get; }
        /// <summary>
        /// Options
        /// </summary>
        ClientWebSocketOptions Options { get; }

        /// <summary>
        /// CloseAsync
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task CloseAsync(WebSocketCloseStatus status, string description, CancellationToken token);
        /// <summary>
        /// CloseOutputAsync
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task CloseOutputAsync(WebSocketCloseStatus status, string description, CancellationToken token);
        /// <summary>
        /// ConnectAsync
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task ConnectAsync(Uri uri, CancellationToken token);
        /// <summary>
        /// ReceiveAsync
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> segment, CancellationToken token);
        /// <summary>
        /// SendAsync
        /// </summary>
        /// <param name="arraySegments"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendAsync(ArraySegment<byte> arraySegments, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token);
    }
}
