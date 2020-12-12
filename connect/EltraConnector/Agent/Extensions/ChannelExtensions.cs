using EltraCommon.Contracts.Channels;
using System.Threading.Tasks;

namespace EltraConnector.Agent.Extensions
{
    /// <summary>
    /// ChannelExtensions
    /// </summary>
    public static class ChannelExtensions
    {
        /// <summary>
        /// Unbind
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static async Task<bool> Unbind(this Channel channel, AgentConnector connector)
        {
            bool result = false;

            if (connector != null)
            {
                result = await connector.UnbindChannel(channel);
            }

            return result;
        }

        /// <summary>
        /// Unbind
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="vcs"></param>
        /// <returns></returns>
        public static async Task<bool> Unbind(this Channel channel, VirtualCommandSet vcs)
        {
            bool result = false;

            var connector = vcs?.Connector;

            if (connector != null)
            {
                result = await connector.UnbindChannel(channel);
            }

            return result;
        }
    }
}
