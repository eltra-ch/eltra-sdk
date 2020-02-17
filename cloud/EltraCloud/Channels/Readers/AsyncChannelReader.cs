using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using EltraCloud.Channels.Interfaces;
using EltraCloudContracts.Contracts.Ws;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Readers
{
    public class AsyncChannelReader : IChannelReader
    {
        #region Private fields
        
        private AsyncMessageReader _readMessageManager;
        private EventWaitHandle _readEvent;

        #endregion

        #region Constructors

        public AsyncChannelReader(IPAddress source, WebSocket webSocket) : base(source, webSocket)
        {
            Timeout = 30;

            _readEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

            _readMessageManager = new AsyncMessageReader(Source, WebSocket);

            _readMessageManager.MessageReceived += OnMessageReceived;
        }

        #endregion

        #region Properties

        public bool IsRunning
        {
            get
            {
                return _readMessageManager.IsRunning;
            }
        }

        public WsMessage Message { get; set; }

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        public int Timeout { get; set; }

        #endregion

        #region Events handling

        private void OnMessageReceived(object sender, Events.ReadMessageEventArgs e)
        {
            Message = e.Message;

            _readEvent.Set();
        }

        #endregion

        #region Methods

        public override Task<WsMessage> Read(CancellationToken cancellationToken)
        {
            WsMessage result = null;

            if (_readEvent.WaitOne(TimeSpan.FromSeconds(Timeout)))
            {
                _readEvent.Reset();

                result = Message;
            }

            return Task.Run(() => { return result; });
        }

        public void Start()
        {
            _readMessageManager?.Start();
        }

        public void Stop()
        {
            _readMessageManager?.Stop();
        }

        #endregion
    }
}
