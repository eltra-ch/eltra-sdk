using System;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Channels;
using EltraCommon.Contracts.Devices;
using Newtonsoft.Json;

namespace EltraConnector.Extensions
{
    internal static class JsonExtensions
    {
        public static string ToJson(this EltraDevice device)
        {
            return JsonConvert.SerializeObject(device);
        }

        public static void FromJson(this EltraDevice device, string json)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            device = JsonConvert.DeserializeObject<EltraDevice>(json);
        }

        public static string ToJson(this ChannelBase channel)
        {
            return JsonConvert.SerializeObject(channel);
        }

        public static void FromJson(this Channel session, string json)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session = JsonConvert.DeserializeObject<Channel>(json);
        }

        public static string ToJson(this EltraDeviceSet sessionDevices)
        {
            return JsonConvert.SerializeObject(sessionDevices);
        }

        public static void FromJson(this EltraDeviceSet sessionDevices, string json)
        {
            if (sessionDevices == null) throw new ArgumentNullException(nameof(sessionDevices));

            sessionDevices = JsonConvert.DeserializeObject<EltraDeviceSet>(json);
        }

        public static string ToJson(this ExecuteCommand execCommand)
        {
            return JsonConvert.SerializeObject(execCommand);
        }
    }
}
