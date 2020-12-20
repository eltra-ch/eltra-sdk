﻿using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Results;
using EltraCommon.Contracts.Users;
using EltraCommon.Logger;
using EltraCommon.Transport;
using EltraConnector.Transport.Definitions;
using EltraConnector.Transport.Events;
using EltraConnector.Transport.Ws.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Rest
{
    class RestConnection : IConnection
    {
        #region Private fields

        private CloudTransporter _cloudTransporter;

        #endregion

        #region Properties

        public CloudTransporter Transporter => _cloudTransporter ?? (_cloudTransporter = new CloudTransporter());

        public string Url { get; set; }
        public string UniqueId { get; set; }
        public string ChannelName { get; set; }
        public bool IsConnected => true;
        public bool IsDisconnecting => false;
        public bool Fallback => false;
        public ConnectionPriority Priority => ConnectionPriority.Low;

        #endregion

        #region Events

        public event EventHandler<ConnectionMessageEventArgs> MessageReceived;
        public event EventHandler<ConnectionMessageEventArgs> MessageSent;
        public event EventHandler<ConnectionMessageEventArgs> ErrorOccured;

        #endregion

        #region Events handling

        protected void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, new ConnectionMessageEventArgs());
        }

        protected void OnMessageSent()
        {
            MessageSent?.Invoke(this, new ConnectionMessageEventArgs());
        }

        protected void OnErrorOccured(HttpStatusCode? httpStatus)
        {
            ErrorOccured?.Invoke(this, new ConnectionMessageEventArgs() { Source = UniqueId, Message = $"http status = {httpStatus}", Type = MessageType.Text });
        }

        #endregion

        #region Methods

        public Task<bool> Connect()
        {
            return Task.FromResult(true);
        }

        public Task<bool> Disconnect()
        {
            return Task.FromResult(true);
        }

        public Task<string> Receive()
        {
            return Task.FromResult(string.Empty);
        }

        public Task<T> Receive<T>()
        {
            return (Task<T>)Task.Run(() => { return null; });
        }

        public Task<bool> Send(UserIdentity identity, string typeName, string data)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;

            try
            {
                string path;
                bool postMethod;

                if (GetPath(obj, out path, out postMethod))
                {
                    if (postMethod)
                    {
                        var requestResult = await Transporter.Post(identity, Url, path, JsonConvert.SerializeObject(obj));

                        if (requestResult.StatusCode == HttpStatusCode.OK)
                        {
                            result = true;

                            OnMessageSent();
                        }
                        else
                        {
                            OnErrorOccured(requestResult?.StatusCode);
                        }
                    }
                    else
                    {
                        var response = await Transporter.Put(identity, Url, path, JsonConvert.SerializeObject(obj));

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var requestResult = JsonConvert.DeserializeObject<RequestResult>(response.Content);

                            result = requestResult.Result;
                        }
                        else
                        {
                            OnErrorOccured(response?.StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        private static bool GetPath<T>(T obj, out string path, out bool postMethod)
        {
            bool result = false;

            path = string.Empty;
            
            postMethod = true;

            if (obj is ExecuteCommandStatus)
            {
                path = "api/command/status";
                result = true;
            }
            else if (obj is ExecuteCommand)
            {
                path = "api/command/push";
                result = true;
            }
            else if (obj is ChannelStatusUpdate)
            {
                path = "api/channel/status";
                postMethod = false;
                result = true;
            }

            return result;
        }

        #endregion
    }
}
