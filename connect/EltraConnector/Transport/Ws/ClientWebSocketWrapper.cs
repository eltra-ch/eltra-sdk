using EltraCommon.Logger;
using System;
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

        private ClientWebSocket _socket;
        private SemaphoreSlim _sendLock;
        private SemaphoreSlim _receiveLock;

        #endregion

        #region Constructors

        public ClientWebSocketWrapper()
        {
            _sendLock = new SemaphoreSlim(1);
            _receiveLock = new SemaphoreSlim(1);

            _socket = new ClientWebSocket();
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
                await _socket.ConnectAsync(uri, token);

                result = true;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CloseAsync", e);
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
                MsgLogger.Exception($"{GetType().Name} - CloseAsync", e);
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
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SendAsync", e);
            }
            finally
            {
                _sendLock.Release();
            }

            return result;
        }

        internal async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> segment, CancellationToken token)
        {
            WebSocketReceiveResult result = null;

            try
            {
                await _receiveLock.WaitAsync();

                result = await _socket.ReceiveAsync(segment, token);
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReceiveAsync", e);
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
                await _sendLock.WaitAsync();

                if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseSent)
                {
                    await _socket.CloseOutputAsync(status, description, token);

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - CloseOutputAsync", $"wrong socket state = {_socket.State}");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CloseOutputAsync", e);
            }
            finally
            {
                _sendLock.Release();
            }

            return result;
        }

        #endregion
    }
}
