using System;

using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Contracts.Devices;
using Newtonsoft.Json;
using EltraCommon.Contracts.Node;

namespace EltraConnector.Extensions
{
    public static class JsonExtensions
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

        public static string ToJson(this Session session)
        {
            return JsonConvert.SerializeObject(session);
        }

        public static void FromJson(this Session session, string json)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session = JsonConvert.DeserializeObject<Session>(json);
        }

        public static string ToJson(this EltraDeviceNodeList sessionDevices)
        {
            return JsonConvert.SerializeObject(sessionDevices);
        }

        public static void FromJson(this EltraDeviceNodeList sessionDevices, string json)
        {
            if (sessionDevices == null) throw new ArgumentNullException(nameof(sessionDevices));

            sessionDevices = JsonConvert.DeserializeObject<EltraDeviceNodeList>(json);
        }

        public static string ToJson(this EltraDeviceNode sessionDevice)
        {
            return JsonConvert.SerializeObject(sessionDevice);
        }

        public static string ToJson(this ExecuteCommand execCommand)
        {
            return JsonConvert.SerializeObject(execCommand);
        }

        public static void FromJson(this EltraDeviceNode sessionDevice, string json)
        {
            if (sessionDevice == null) throw new ArgumentNullException(nameof(sessionDevice));

            sessionDevice = JsonConvert.DeserializeObject<EltraDeviceNode>(json);
        }
    }
}
