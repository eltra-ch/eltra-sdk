using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.SyncAgent;
using EltraConnector.UserAgent.Vcs;
using System;
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
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(MasterDevice device, uint updateInterval, uint timeout)
            : base(device.CloudAgent.Url, device.CloudAgent.ChannelId, device.CloudAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="masterAgent"></param>
        /// <param name="device"></param>
        /// <param name="updateInterval"></param>
        /// <param name="timeout"></param>
        public MasterVcs(SyncCloudAgent masterAgent, EltraDevice device, uint updateInterval, uint timeout)
            : base(masterAgent.Url, device.ChannelId, masterAgent.Identity, updateInterval, timeout)
        {
            Device = device;
        }

        /// <summary>
        /// MasterVcs
        /// </summary>
        /// <param name="device"></param>
        public MasterVcs(MasterDevice device)
            : base(device.CloudAgent, device, device.CloudAgent.UpdateInterval, device.CloudAgent.Timeout)
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
                    MsgLogger.WriteError($"{GetType().Name} - ReadAllParameters", $"set parameter ({parameterEntry.Index}:{parameterEntry.SubIndex}) value failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - ReadAllParameters", $"get parameter ({parameterEntry.Index}:{parameterEntry.SubIndex}) value failed!");
            }

            return result;
        }
    }
}
