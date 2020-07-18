using EltraCommon.Contracts.Channels;

namespace EltraConnector.Controllers.Base
{
    public class CloudChannelControllerAdapter : CloudControllerAdapter
    {
        #region Constructors

        public CloudChannelControllerAdapter(string url, Channel channel)
            :base(url)
        {
            Channel = channel;
        }

        #endregion

        #region Properties
        
        protected Channel Channel { get; }

        #endregion
    }
}
