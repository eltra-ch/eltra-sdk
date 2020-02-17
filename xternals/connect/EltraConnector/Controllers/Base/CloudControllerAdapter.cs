using EltraConnector.Transport;

namespace EltraConnector.Controllers.Base
{
    public class CloudControllerAdapter
    {
        #region Constructors

        public CloudControllerAdapter(string url)
        {
            Url = url;
            Transporter = new CloudTransporter();
        }

        #endregion

        #region Properties

        public string Url { get; }

        protected CloudTransporter Transporter { get; }

        #endregion

        #region Methods

        public virtual bool Start()
        {
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        #endregion
    }
}
