﻿using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Transport;
using EltraConnector.SyncAgent;
using EltraConnector.Transport.Udp;
using EltraConnector.Transport.Ws;
using EltraConnector.UserAgent.Vcs;
using System.Threading.Tasks;

namespace EltraConnector.Master.Device
{
    /// <summary>
    /// MasterVcs
    /// </summary>
    public class MasterVcs : DeviceVcs
    {
        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="udpClient"></param>
        /// <param name="webSocketClient"></param>
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient, MasterDevice device, uint updateInterval, uint timeout)
            : base(httpClient, udpClient, webSocketClient, device.CloudAgent.Url, device.CloudAgent.ChannelId, device.CloudAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="udpClient"></param>
        /// <param name="webSocketClient"></param>
        /// <param name="masterAgent"></param>
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(IHttpClient httpClient, IUdpClient udpClient, IWebSocketClient webSocketClient, SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(httpClient, udpClient, webSocketClient, masterAgent.Url, device.ChannelId, masterAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="udpClient"></param>
        /// <param name="device"></param>
        public MasterVcs(IHttpClient httpClient, IUdpClient udpClient, MasterDevice device)
            : base(httpClient, udpClient, device.CloudAgent, device, device.CloudAgent.UpdateInterval, device.CloudAgent.Timeout)
        {
            Device = device;
        }

        /// <summary>
        /// ReadAllParameters
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReadAllParameters()
        {
            bool result = true;
            var objectDictionary = Device?.ObjectDictionary;

            if (objectDictionary != null)
            {
                foreach (var parameter in objectDictionary.Parameters)
                {
                    if (parameter is Parameter parameterEntry)
                    {
                        result = await ReadParameter(parameterEntry);
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                        {
                            foreach (var subParameter in subParameters)
                            {
                                if (subParameter is Parameter subParameterEntry)
                                {
                                    result = await ReadParameter(subParameterEntry);
                                }   
                            }
                        }
                    }
                }
            }

            return result;
        }

        private async Task<bool> ReadParameter(Parameter parameterEntry)
        {
            bool result = false;
            var parameterValue = await Agent.GetParameterValue(Device.ChannelId, Device.NodeId, parameterEntry.Index, parameterEntry.SubIndex);

            if (parameterValue != null)
            {
                result = parameterEntry.SetValue(parameterValue);

                if (!result)
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadAllParameters", $"set parameter ({parameterEntry.Index:X4}:{parameterEntry.SubIndex:X2}) value failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - ReadAllParameters", $"get parameter ({parameterEntry.Index:X4}:{parameterEntry.SubIndex:X2}) value failed!");
            }

            return result;
        }
    }
}
