using EltraCommon.Logger;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Ws
{
    /*
     https://www.codetinkerer.com/2018/06/05/aspnet-core-websockets.html
    
    Thread-safety:

        It’s acceptable to call ReceiveAsync and SendAsync in parallel. One of each may run concurrently.
        It’s acceptable to have a pending ReceiveAsync while CloseOutputAsync or CloseAsync is called.
        
        Attempting to invoke any other operations in parallel may corrupt the instance. 
        Attempting to invoke a send operation while another is in progress or a receive operation while another is in progress will result in an exception.
     */

    class ClientWebSocketWrapper
    {
        #region Private fields

        private readonly IWebSocketClient _socket;
        private readonly SemaphoreSlim _sendLock;
        private readonly SemaphoreSlim _receiveLock;

        #endregion

        #region Constructors

        public ClientWebSocketWrapper(IWebSocketClient clientWebSocket)
        {
            _socket = clientWebSocket;
            _sendLock = new SemaphoreSlim(1);
            _receiveLock = new SemaphoreSlim(1);

            Initialize();
        }

        #endregion

        #region Properties

        public WebSocketState State => _socket.State;

        public WebSocketCloseStatus? CloseStatus => _socket.CloseStatus;

        #endregion

        #region Methods

        internal async Task<bool> ConnectAsync(Uri uri, CancellationToken token)
        {
            bool result = false;

            try
            {
                if (_socket.State != WebSocketState.Open && _socket.State != WebSocketState.Connecting)
                {
                    await _socket.ConnectAsync(uri, token);
                }

                result = true;
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(OperationCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - ConnectAsync", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - ConnectAsync", "close requested");
                }
            }

            return result;
        }

        internal async Task<bool> CloseAsync(WebSocketCloseStatus status, string description, CancellationToken token)
        {
            bool result = false;

            try
            {
                await _sendLock.WaitAsync();

                await _socket.CloseAsync(status, description, token);

                result = true;
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(TaskCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - CloseAsync", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - CloseAsync", "close requested");
                }
            }
            finally
            {
                _sendLock.Release();
            }

            return result;
        }

        internal async Task<bool> SendAsync(ArraySegment<byte> arraySegments, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token)
        {
            bool result = false;

            try
            {
                await _sendLock.WaitAsync();

                await _socket.SendAsync(arraySegments, messageType, endOfMessage, token);

                result = true;
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - SendAsync, error code = {e.WebSocketErrorCode}", e);

                await HandleWebSocketException(e, token);
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(TaskCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - SendAsync", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - SendAsync", "close requested");
                }
            }
            finally
            {
                _sendLock.Release();
            }

            return result;
        }

        private async Task HandleWebSocketException(WebSocketException e, CancellationToken token)
        {
            var errorCode = e.WebSocketErrorCode;

            if (errorCode == WebSocketError.ConnectionClosedPrematurely ||
                errorCode == WebSocketError.Faulted || 
                errorCode == WebSocketError.InvalidState)
            {
                MsgLogger.WriteError($"{GetType().Name} - HandleWebSocketException", "socket error, close socket");

                await CloseOutputAsync(WebSocketCloseStatus.Empty, string.Empty, token);

                Initialize();
            }
        }

        internal async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> segment, CancellationToken token)
        {
            WebSocketReceiveResult result = null;

            try
            {
                await _receiveLock.WaitAsync();

                result = await _socket.ReceiveAsync(segment, token);
            }
            catch(WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReceiveAsync - error code = {e.WebSocketErrorCode}", e);

                await HandleWebSocketException(e, token);
            }
            catch(Exception e)
            {
                if (e.GetType() != typeof(TaskCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - ReceiveAsync", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - ReceiveAsync", "close requested");
                }
            }
            finally
            {
                _receiveLock.Release();
            }

            return result;
        }

        internal async Task<bool> CloseOutputAsync(WebSocketCloseStatus status, string description, CancellationToken token)
        {
            bool result = false;

            try
            {
                if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseSent)
                {
                    await _socket.CloseOutputAsync(status, description, token);

                    result = true;
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - CloseOutputAsync", $"wrong socket state = {_socket.State}");
                }
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(TaskCanceledException))
                {
                    MsgLogger.Exception($"{GetType().Name} - CloseOutputAsync", e);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - CloseOutputAsync", "close requested");
                }
            }

            return result;
        }

        private void Initialize()
        {
            const string methodName = "Initialize";
            
            try
            {
                if (_socket != null)
                {
                    var opt = _socket.Options;

                    if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseSent)
                    {
                        opt.UseDefaultCredentials = true;
                    }
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {methodName}", e);
            }
        }

        #endregion
    }
}
