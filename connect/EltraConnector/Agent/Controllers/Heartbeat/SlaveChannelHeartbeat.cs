using EltraConnector.Agent.Controllers.Commands;
using EltraConnector.Channels;
using EltraConnector.Controllers.Base;

namespace EltraConnector.Agent.Controllers.Heartbeat
{
    class SlaveChannelHeartbeat : ChannelHeartbeat
    {
        public SlaveChannelHeartbeat(ChannelControllerAdapter channelControllerAdapter, SlaveExecuteCommander executeCommander, uint updateInterval, uint timeout) 
            : base(channelControllerAdapter, executeCommander.WsChannelName, executeCommander.WsChannelId, updateInterval, timeout)
        {
        }
    }
}
