using EltraCommon.Contracts.Channels;

namespace EltraConnector.Controllers.Base
{
    public class CloudSessionControllerAdapter : CloudControllerAdapter
    {
        #region Constructors

        public CloudSessionControllerAdapter(string url, Channel session)
            :base(url)
        {
            Channel = session;
        }

        #endregion

        #region Properties
        
        protected Channel Channel { get; }

        #endregion
    }
}
