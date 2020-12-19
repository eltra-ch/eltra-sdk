﻿using EltraConnector.Transport.Udp.Response;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpServer : EltraUdpConnector
    {
        #region Private fields
        
        private Task _listenerTask;
        
        #endregion

        #region Constructors

        public EltraUdpServer()
        {                 
        }

        #endregion

        #region Properties

        public bool IsRunning
        {
            get
            {
                return !(_listenerTask == null || _listenerTask.IsCompleted);
            }
        }

        #endregion

        #region Methods

        protected override UdpClient CreateUdpClient()
        {
            var result = new UdpClient(new IPEndPoint(IPAddress.Parse(Host), Port));

            return result;
        }

        protected virtual async Task<int> ProcessReceivedMessage(ReceiveResponse receiveResponse)
        {
            return await Send(receiveResponse.Endpoint, receiveResponse.Text);
        }

        public bool Start()
        {
            if (!IsRunning)
            {
                _listenerTask = Task.Run(async () =>
                {
                    int minWaitTime = 10;
                    
                    do
                    {
                        var receiveResponse = await Receive();

                        if (receiveResponse != null)
                        {
                            _ = Task.Run(async ()=>
                            {
                                OnMessageReceived(receiveResponse);

                                if (!receiveResponse.Handled)
                                {
                                    await ProcessReceivedMessage(receiveResponse);
                                }
                            });                            
                        }
                        else
                        {
                            await Task.Delay(minWaitTime);
                        }
                    }
                    while (!IsCanceled);
                });
            }

            return !_listenerTask.IsCompleted;
        }

        public bool Stop()
        {
            bool result = false;

            if (IsRunning)
            {
                Cancel();

                _listenerTask.Wait();

                _listenerTask = null;

                result = true;
            }

            return result;
        }

        #endregion
    }
}
