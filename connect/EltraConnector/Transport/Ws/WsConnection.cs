using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EltraConnector.Transport.Ws.Interfaces;
using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.Ws;
using EltraCommon.Logger;
using System.Text.Json;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Ws.Converters;
using EltraConnector.Transport.Definitions;
using EltraCommon.Extensions;
using EltraCommon.Helpers;

namespace EltraConnector.Transport.Ws
{
    class WsConnection : IConnection
    {
        #region Private fields

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _disconnectTokenSource;
        private string _url;
        private SemaphoreSlim _receiveLock;

        #endregion

        #region Constructors

        public WsConnection(string uniqueId, string channelName)
        {
            BufferSize = 4096;

            Initialize();

            UniqueId = uniqueId;
            ChannelName = channelName;
        }

        #endregion

        #region Properties

        public int BufferSize { get; set; }

        public ClientWebSocketWrapper Socket { get; private set; }
        
        public string UniqueId { get; set; }
        
        public string ChannelName { get; set; }

        public bool Fallback => false;

        public string Url 
        { 
            get => _url;
            set 
            {
                _url = WsHostUrlConverter.ToWsUrl(value);
            }
        }
        
        public bool IsConnected
        {
            get
            {
                bool result = false;

                try
                {
                    result = (Socket.State == WebSocketState.Open);
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - IsConnected", e);
                }

                return result;
            }
        }

        public bool IsDisconnecting
        {
            get
            {
                bool result = false;

                if (_disconnectTokenSource != null)
                {
                    if(_disconnectTokenSource.IsCancellationRequested)
                    {
                        if(Socket.State == WebSocketState.Open ||
                            Socket.State == WebSocketState.CloseSent ||
                            Socket.State == WebSocketState.CloseReceived)
                        {
                            result = true;
                        }
                    }
                }

                return result;
            }
        }

        public WebSocketCloseStatus? LastCloseStatus { get; set; }

        public ConnectionPriority Priority => ConnectionPriority.Normal;

        public bool ReceiveSupported => true;

        #endregion

        #region Events

        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        private void OnMessageReceived(WsMessage wsMessage)
        {
            if (wsMessage != null)
            {
                MessageReceived?.Invoke(this, new ConnectionMessageEventArgs() 
                { Source = UniqueId, Message = JsonSerializer.Serialize(wsMessage), Type = MessageType.WsMessage });
            }
        }

        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = message, Type = MessageType.Text });
        }

        private void OnMessageDataReceived(WsMessage wsMessage)
        {
            if (wsMessage != null)
            {
                ConnectionMessageEventArgs args = new ConnectionMessageEventArgs { Source = UniqueId, Type = MessageType.Data };

                var data = wsMessage.Data;

                if(!string.IsNullOrEmpty(data) && data != "ACK" && data != "KEEPALIVE")
                {
                    args.Message = data.FromBase64();
                }
                else
                {
                    args.Message = data;
                }

                MessageReceived?.Invoke(this, args);
            }
        }

        private void OnMessageSent(string msg, MessageType type)
        {
            MessageSent?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = msg, Type = type });
        }

        private void OnErrorOccured(string msg, MessageType type)
        {
            ErrorOccured?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = msg, Type = type });
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            Socket = new ClientWebSocketWrapper();
            
            LastCloseStatus = null;

            _cancellationTokenSource = new CancellationTokenSource();
            _disconnectTokenSource = new CancellationTokenSource();
            _receiveLock = new SemaphoreSlim(1);
        }

        public async Task<bool> Connect()
        {
            bool result = false;

            try
            {
                Initialize();

                await Socket.ConnectAsync(new Uri(Url), _cancellationTokenSource.Token);

                result = (Socket.State == WebSocketState.Open);
            }
            catch(WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Connect", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Connect", e);
            }

            return result;
        }

        public Task<bool> Listen()
        {
            return Task.FromResult(false);
        }

        public async Task<bool> Disconnect()
        {
            bool result = false;

            try
            {
                if (IsConnected)
                {
                    if (!_disconnectTokenSource.IsCancellationRequested)
                    {
                        _disconnectTokenSource.Cancel();
                    }

                    _cancellationTokenSource.Cancel();

                    if (Socket.State == WebSocketState.Open)
                    {
                        await Socket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                    }
                    else if (Socket.State == WebSocketState.Closed)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Disconnect", $"Disconnect, state = {Socket.State}");
                    }

                    if (Socket.State == WebSocketState.Closed)
                    {
                        result = true;
                    }
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Disconnect", $"Disconnect, state = {Socket.State}");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Disconnect", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                unchecked
                {
                    if (e.HResult == (int)0x80131620)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Disconnect", "connection failed, reconnect");

                        Initialize();
                    }
                    else
                    {
                        MsgLogger.Exception($"{GetType().Name} - Disconnect", e);
                    }
                }
            }

            return result;
        }

        public async Task<bool> Send(UserIdentity identity, string typeName, string data)
        {
            bool result = false;

            try
            {
                var base64data = data.ToBase64();
                var wsMessage = new WsMessage()
                {
                    Identity = identity,
                    ChannelName = ChannelName,
                    TypeName = typeName,
                    Data = base64data,
                    Checksum = CryptHelpers.ToMD5(base64data)
                };

                var json = JsonSerializer.Serialize(wsMessage);
                var buffer = Encoding.UTF8.GetBytes(json);

                if (!Socket.CloseStatus.HasValue && Socket.State == WebSocketState.Open)
                {
                    await Socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

                    result = true;
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);

                OnErrorOccured("send failed - exception", MessageType.Text);
            }

            return result;
        }

        private async void HandleBrokenConnection(WebSocketError errorCode)
        {
            MsgLogger.WriteError($"{GetType().Name} - HandleBrokenConnection", $"Connection to '{Url}' failed, error code = {errorCode}");

            if (errorCode == WebSocketError.ConnectionClosedPrematurely && Socket.State == WebSocketState.Open)
            {
                MsgLogger.WriteLine("try recover connection ...");

                Initialize();
            }
            else
            {
                OnErrorOccured("broken connection, delay 100 ms", MessageType.Text);

                await Task.Delay(100);
            }            
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;

            try
            {
                var msg = JsonSerializer.Serialize(obj);

                if (await Send(identity, typeof(T).FullName, msg))
                {
                    OnMessageSent(msg, MessageType.Json);

                    result = true;
                }
                else
                {
                    OnErrorOccured("send error occured", MessageType.Text);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);

                OnErrorOccured("send error occured - exception", MessageType.Text);
            }

            return result;
        }

        private async Task<WsMessage> ReadWsMessage()
        {
            WsMessage result = null;

            var textMsg = await ReadMessage();

            if (!string.IsNullOrEmpty(textMsg))
            {
                var wsMsg = textMsg.TryDeserializeObject<WsMessage>();

                if (wsMsg is WsMessage)
                {
                    if(wsMsg.TypeName == typeof(WsMessage).FullName)
                    {
                        var base64Data = wsMsg.Data;

                        if (!string.IsNullOrEmpty(base64Data))
                        {
                            var json = base64Data.FromBase64();

                            result = json.TryDeserializeObject<WsMessage>();
                        }
                    }
                    else
                    {
                        result = wsMsg;
                    }

                    OnMessageReceived(result);
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name}", $"received non ws message, {textMsg}");

                    OnMessageReceived(textMsg);
                }
            }

            return result;
        }

        private async Task<string> ReadMessage()
        {
            string result = string.Empty;
            int bufferSize = BufferSize;
            var buffer = new byte[bufferSize];
            var segment = new ArraySegment<byte>(buffer);
            var fullBuffer = new byte[bufferSize];
            int offset = 0;
            int fullBufferSize = 0;

            try
            {
                if (Socket.State == WebSocketState.Open)
                {
                    await _receiveLock.WaitAsync();

                    var receiveResult = await Socket.ReceiveAsync(segment, _cancellationTokenSource.Token);

                    while (receiveResult != null && 
                          (receiveResult.CloseStatus == null || (receiveResult.CloseStatus != null && !receiveResult.CloseStatus.HasValue)))
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

                            result = Encoding.UTF8.GetString(fullBuffer, 0, fullBufferSize);

                            if (!string.IsNullOrEmpty(result))
                            {
                                break;
                            }

                            buffer = new byte[bufferSize];
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

                            offset += receiveResult.Count;
                            fullBufferSize += receiveResult.Count;
                        }

                        if (Socket.State == WebSocketState.Open)
                        {
                            receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ReadMessage", $"cannot read - socket state = {Socket.State}");
                            break;
                        }
                    }

                    if(receiveResult == null || (receiveResult.MessageType == WebSocketMessageType.Close &&
                        (Socket.State == WebSocketState.CloseReceived || Socket.State == WebSocketState.Open)))
                    {
                        await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Server closed connection", _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - ReadMessage", $"cannot read - socket state = {Socket.State}");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e.InnerException != null ? e.InnerException : e);

                OnErrorOccured($"read message failed - exception, error code = {e.NativeErrorCode}", MessageType.Text);
            }
            catch (InvalidOperationException e)
            {               
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e);

                OnErrorOccured("read message failed - invalid operation", MessageType.Text);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e);

                OnErrorOccured($"read message failed - exception = '{e.Message}'", MessageType.Text);
            }
            finally
            {
                _receiveLock.Release();
            }

            return result;
        }

        public async Task<string> Receive()
        {
            string result = string.Empty;
            
            try
            {
                var wsMsg = await ReadWsMessage();

                if(wsMsg != null && (wsMsg.TypeName == typeof(WsMessageAck).FullName || 
                   wsMsg.TypeName == typeof(WsMessageKeepAlive).FullName))
                {
                    result = wsMsg.Data;

                    OnMessageDataReceived(wsMsg);
                }
                else if(wsMsg != null)
                {
                    if (await Send(new UserIdentity(), new WsMessageAck()))
                    {
                        result = wsMsg.Data.FromBase64();
                        LastCloseStatus = null;
                    }
                    
                    OnMessageDataReceived(wsMsg);
                }
                else if (IsConnected)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Receive", $"receive ws message failed!");

                    OnErrorOccured($"non ws message received", MessageType.Text);
                }
                else
                {
                    MsgLogger.WriteLine($"receive ws message skipped, socket state = {Socket.State}!");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);

                HandleBrokenConnection(e.WebSocketErrorCode);

                OnErrorOccured($"receive failed, error code = {e.WebSocketErrorCode}", MessageType.Text);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);

                OnErrorOccured($"receive failed, exception", MessageType.Text);
            }

            return result;
        }

        public static bool IsJson(string json)
        {
            json = json.Trim();

            return json.StartsWith("{") && json.EndsWith("}")
                   || json.StartsWith("[") && json.EndsWith("]");
        }

        public async Task<T> Receive<T>()
        {
            T result = default;

            try
            {
                string msg = string.Empty;

                do
                {
                    msg = await Receive();
                }
                while ((msg == "ACK" || msg == "KEEPALIVE") && !_cancellationTokenSource.IsCancellationRequested);
                
                if (IsJson(msg))
                {                 
                    result = msg.TryDeserializeObject<T>();                    
                }
                else if(string.IsNullOrEmpty(msg) && IsConnected)
                {                    
                    MsgLogger.WriteLine($"empty string received, close status='{Socket.CloseStatus}'");

                    OnErrorOccured("receive error occured - empty string", MessageType.Text);
                }                
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);

                OnErrorOccured("receive error occured", MessageType.Text);
            }

            return result;
        }

        #endregion
    }
}
