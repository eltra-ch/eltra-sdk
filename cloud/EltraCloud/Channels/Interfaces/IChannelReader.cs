using EltraCloudContracts.Contracts.Ws;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace EltraCloud.Channels.Interfaces
{
    public abstract class IChannelReader
    {
        #region Constructors

        public IChannelReader(IPAddress source, WebSocket webSocket)
        {
            BufferSize = 4096;

            WebSocket = webSocket;
            Source = source;
            LastMessageType = WebSocketMessageType.Text;            
        }

        #endregion

        #region Properties

        public WebSocket WebSocket { get; set; }

        public WebSocketMessageType LastMessageType { get; set; }

        protected IPAddress Source { get; set; }

        public bool IsCloseRequested
        {
            get
            {
                return LastMessageType == WebSocketMessageType.Close;
            }
        }

        public int BufferSize { get; set; }

        #endregion

        #region Methods

        public abstract Task<WsMessage> Read(CancellationToken cancellationToken);

        #endregion
    }
}
