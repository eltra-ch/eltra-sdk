using System;
using System.Collections.Specialized;

namespace EltraCommon.Helpers
{
    public static class UrlHelper
    {
        public static string BuildUrl(string url, string path, NameValueCollection query)
        {
            var builder = new UriBuilder(url) { Path = path, Query = query.ToString() };

            return builder.ToString();
        }
    }
}
