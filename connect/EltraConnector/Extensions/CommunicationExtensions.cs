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
                var msg = data.Trim(new char[] { '\"' });

                if (msg == "ACK" || msg == "KEEPALIVE")
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
