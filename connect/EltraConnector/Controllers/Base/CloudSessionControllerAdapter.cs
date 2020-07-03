using EltraCommon.Contracts.Sessions;

namespace EltraConnector.Controllers.Base
{
    public class CloudSessionControllerAdapter : CloudControllerAdapter
    {
        #region Constructors

        public CloudSessionControllerAdapter(string url, Session session)
            :base(url)
        {
            Session = session;
        }

        #endregion

        #region Properties
        
        protected Session Session { get; }

        #endregion
    }
}
