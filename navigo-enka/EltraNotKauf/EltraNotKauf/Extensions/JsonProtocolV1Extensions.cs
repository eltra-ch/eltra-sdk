using EltraCloudContracts.Enka.Orders;
using Newtonsoft.Json;

namespace EltraNotKauf.Extensions
{
    public static class JsonProtocolV1Extensions
    {
        public static string ToJson(this JsonProtocolV1 message)
        {
            return JsonConvert.SerializeObject(message);
        }        
    }
}
