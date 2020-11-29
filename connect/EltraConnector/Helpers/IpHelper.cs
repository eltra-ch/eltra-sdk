using EltraCommon.Logger;
using System;
using System.Net;
using System.Net.Sockets;

namespace EltraConnector.Helpers
{
    internal static class IpHelper
    {
        public static string GetLocalIpAddress()
        {
            const string host = "8.8.8.8";

            string result = string.Empty;

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect(host, 65530);

                    var endPoint = socket.LocalEndPoint as IPEndPoint;

                    if (endPoint != null)
                    {
                        result = endPoint.Address.ToString();
                    }
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"IpHelper - GetLocalIpAddress", e);
            }

            return result;
        }

    }
}
