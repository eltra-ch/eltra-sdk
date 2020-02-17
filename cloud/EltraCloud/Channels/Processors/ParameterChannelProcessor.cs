using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EltraCloud.Services;
using EltraCloud.Services.Events;
using EltraCloud.Channels.Interfaces;
using EltraCloud.Channels.Readers;
using EltraCloudContracts.Contracts.Sessions;
using EltraCloudContracts.Contracts.Ws;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;
using Newtonsoft.Json;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Processors
{
    public class ParameterChannelProcessor : IChannelProcessor
    {
        #region Private fields

        private readonly object _lock = new object();

        private SessionIdentification _sessionIdentification;
        private string _channelName;
        private Session _session;
        private readonly Stack<Parameter> _changedParameters;
        private readonly AsyncChannelReader _asyncChannelReader;

        #endregion

        #region Constructors

        public ParameterChannelProcessor(IPAddress source, WebSocket webSocket, ISessionService sessionService)
            : base(source, webSocket, sessionService)
        {
            _changedParameters = new Stack<Parameter>();

            _asyncChannelReader = new AsyncChannelReader(source, webSocket);

            Reader = _asyncChannelReader;
        }

        #endregion

        #region Events handling

        private void OnSessionStatusChanged(object sender, SessionStatusChangedEventArgs e)
        {
            MsgLogger.Print($"session '{e.Uuid}' status changed to {e.Status}!");

            if (e.Uuid == _sessionIdentification.Uuid && e.Status == SessionStatus.Offline)
            {
                _asyncChannelReader.Stop();
            }
        }

        private void OnParameterValueChanged(object sender, ParameterValueChangedEventArgs e)
        {
            try
            {
                MsgLogger.WriteDebug($"{GetType().Name} - OnParameterValueChanged", $"Send response to channel '{_channelName}'");

                if(_sessionIdentification!=null)
                {
                    string sessionUuid = _sessionIdentification.Uuid;
                    var parameterUpdate = e.ParameterUpdate;

                    if(parameterUpdate!=null && parameterUpdate.SessionUuid != sessionUuid)
                    { 
                        try
                        {
                            bool masterSession = SessionService.DeviceExists(sessionUuid, parameterUpdate.SerialNumber);

                            if (masterSession || SessionService.IsDeviceUsedByAgent(sessionUuid, parameterUpdate.SerialNumber))
                            { 
                                var parameter = parameterUpdate.Parameter;

                                if(parameter!=null)
                                {
                                    lock(_lock)
                                    {
                                        _changedParameters.Push(parameter);
                                    }
                                }
                                else
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - OnParameterValueChanged", $"Session = {sessionUuid}, parameter update, parameter is null!");
                                }
                            }
                            else
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - OnParameterValueChanged", $"Device 0x{parameterUpdate.SerialNumber:X4} is not used by session = {sessionUuid}");
                            }
                        }
                        catch(Exception ex)
                        {
                            MsgLogger.Exception($"{GetType().Name} - OnParameterValueChanged", ex);
                        }

                    }
                    else if (parameterUpdate != null)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - OnParameterValueChanged", $"Session = {sessionUuid}, parameter update, parameter update for different session {parameterUpdate.SessionUuid}!");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - OnParameterValueChanged", $"Session = {sessionUuid}, parameter update, parameter update is null!");
                    }
                }
            }
            catch (WebSocketException ex)
            {
                MsgLogger.WriteError($"{GetType().Name} - OnParameterValueChanged", $"parameter update exception, error code={ex.ErrorCode}, ws error code={ex.WebSocketErrorCode}");
            }
            catch (Exception ex)
            {
                MsgLogger.Exception($"{GetType().Name} - OnParameterValueChanged", ex);
            }            
        }

        #endregion

        #region Methods

        public override async Task<bool> ProcessMsg(WsMessage msg)
        {
            bool result = false;

            RegisterEvents();

            if (msg !=null && msg.TypeName == typeof(SessionIdentification).FullName)
            {
                _channelName = msg.ChannelName;

                _sessionIdentification = JsonConvert.DeserializeObject<SessionIdentification>(msg.Data);

                if(_sessionIdentification!=null)
                {
                    try
                    {
                        _asyncChannelReader.Start();

                        MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"send invite ACK in channel '{msg.ChannelName}' to {Source?.ToString()}");

                        if (await Send(new WsMessageAck()))
                        {
                            MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"parameter update, channel='{_channelName}', session='{_sessionIdentification.Uuid}'");

                            _session = SessionService.GetSession(_sessionIdentification.Uuid);

                            if(_session!=null)
                            {
                                result = true;
                            }

                            const int waitIntervalInMs = 10;
                            const int validateDelay = 3000;

                            var keepAliveManager = new KeepAliveManager(this);
                            
                            keepAliveManager.Start();

                            while (WebSocket.State == WebSocketState.Open &&
                                    !WebSocket.CloseStatus.HasValue &&
                                    _session != null && _session.Status == SessionStatus.Online && result && 
                                    keepAliveManager.IsRunning && _asyncChannelReader.IsRunning)
                            {
                                var watch = new Stopwatch();

                                watch.Start();

                                while (watch.ElapsedMilliseconds < validateDelay && WebSocket.State == WebSocketState.Open && result)
                                {
                                    Parameter parameter = null;

                                    lock (_lock)
                                    {
                                        _changedParameters.TryPop(out parameter);
                                    }

                                    if (parameter != null)
                                    {
                                        if (await Send(_channelName, parameter))
                                        {
                                            MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"Parameter '{parameter.UniqueId}' update send to channel '{_channelName}'");                                            
                                        }
                                        else
                                        {
                                            MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"Parameter '{parameter.UniqueId}' update send failed! channel '{_channelName}'");
                                            result = false;
                                        }
                                    }

                                    if (result)
                                    {                                        
                                        await Task.Delay(waitIntervalInMs);                                        
                                    }
                                }

                                _session = SessionService.GetSession(_sessionIdentification.Uuid);
                            }

                            keepAliveManager.Stop();

                            MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"parameter update channel, channel='{_channelName}' closing, session='{_sessionIdentification.Uuid}'");
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"ACK send failed! channel '{msg.ChannelName}' exception");
                        }

                        _asyncChannelReader.Stop();
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
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ProcessMsg", $"channel '{msg.ChannelName}' session identification failed!");
                }
            }

            UnregisterEvents();

            MsgLogger.WriteDebug($"{GetType().Name} - ProcessMsg", $"exit channel '{msg.ChannelName}'");

            return result;
        }

        private void RegisterEvents()
        {
            SessionService.SessionStatusChanged += OnSessionStatusChanged;
            SessionService.ParameterValueChanged += OnParameterValueChanged;
        }

        private void UnregisterEvents()
        {
            SessionService.ParameterValueChanged -= OnParameterValueChanged;
            SessionService.SessionStatusChanged -= OnSessionStatusChanged;
        }

        #endregion
    }
}
