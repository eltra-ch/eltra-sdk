using System;
using System.Net;

namespace StreemaMaster.Site
{
    class NodeWebClient : WebClient
    {
        public NodeWebClient()
        {
            Timeout = 60000;
        }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);

            if (w != null)
            {
                w.Timeout = Timeout;
            }

            return w;
        }
    }
}
