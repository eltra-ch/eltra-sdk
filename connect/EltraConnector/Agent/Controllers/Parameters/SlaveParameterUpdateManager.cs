using EltraConnector.Agent.Controllers.Commands;
using EltraConnector.Agent.Parameters;
using EltraConnector.Controllers.Base;

namespace EltraConnector.Agent.Controllers.Parameters
{
    internal class SlaveParameterUpdateManager : ParameterUpdateManager
    {
        public SlaveParameterUpdateManager(ChannelControllerAdapter channelAdapter, SlaveExecuteCommander commander) 
            : base(channelAdapter, commander.WsChannelName, commander.WsChannelId)
        {
        }

        public SlaveParameterUpdateManager(ChannelControllerAdapter channelAdapter, int nodeId, SlaveExecuteCommander commander)
            : base(channelAdapter, nodeId, commander.WsChannelName, commander.WsChannelId)
        {
        }
    }
}
