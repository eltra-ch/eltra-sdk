using System;
using System.Net;

namespace EltraConnector.Transport
{
    internal class TransporterResponse
    {
        public TransporterResponse()
        {
            StatusCode = HttpStatusCode.NotImplemented;
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public Exception Exception { get; set; }
    }
}
