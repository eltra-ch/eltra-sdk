namespace EltraConnector.Transport.Ws.Converters
{
    static class WsHostUrlConverter
    {
        public static string ToWsUrl(string url)
        {
            string result = url;

            if(url.StartsWith("http"))
            { 
                result = url.Replace("http", "ws");
            }
            else if (url.StartsWith("https"))
            {
                result = url.Replace("https", "wss");
            }

            if(result.EndsWith("/"))
            {
                result += "ws";
            }
            else
            {
                result += "/ws";
            }

            return result;
        }

    }
}
