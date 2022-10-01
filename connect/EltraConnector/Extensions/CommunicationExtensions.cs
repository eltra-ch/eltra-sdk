using EltraCommon.Contracts.Ws;

namespace EltraConnector.Extensions
{
    internal static class CommunicationExtensions
    {
        public static bool IsControlMessage(this WsMessage message)
        {
            bool result = false;
            var data = message.Data;

            if (!string.IsNullOrEmpty(data))
            {
                result = data.IsControlMessage();
            }

            return result;
        }

        public static bool IsControlMessage(this string data)
        {
            bool result = false;
            
            if (!string.IsNullOrEmpty(data))
            {
                var msg = data.Trim(new char[] { '\"' });

                if (msg == WsMessageAck.RawData || msg == WsMessageKeepAlive.RawData)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
