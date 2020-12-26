using EltraConnector.Channels;
using EltraConnector.Controllers.Base;
using EltraConnector.Master.Controllers.Commands;

namespace EltraConnector.Master.Controllers.Heartbeat
{
    class MasterChannelHeartbeat : ChannelHeartbeat
    {
        public MasterChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, MasterCommandExecutor commandExecutor, uint updateInterval, uint timeout) 
            : base(channelControllerAdapter, commandExecutor.WsChannelName, commandExecutor.WsChannelId, updateInterval, timeout)
        {
        }
    }
}
