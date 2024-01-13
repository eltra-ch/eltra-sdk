using EltraConnector.Transport.Udp.Response;
using System.Threading.Tasks;

namespace EltraConnector.Transport.Udp
{
    internal class EltraUdpServer : EltraUdpConnector
    {
        #region Private fields

        private readonly IUdpClient _udpClient;

        private Task _listenerTask;
        
        #endregion

        #region Constructors

        public EltraUdpServer(IUdpClient udpClient)
            : base(udpClient)
        {
            _udpClient = udpClient;
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

        protected override UdpClientWrapper CreateUdpClient()
        {
            var result = new UdpClientWrapper(_udpClient, Host, Port);

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
                Abort();

                _listenerTask.Wait();

                _listenerTask = null;

                result = true;
            }

            return result;
        }

        #endregion
    }
}
