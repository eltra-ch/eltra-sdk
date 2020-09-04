using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Base;
using EltraCommon.Helpers;
using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using Newtonsoft.Json;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.History;
using System.Net;

namespace EltraConnector.Controllers
{
    internal class ParameterControllerAdapter : CloudChannelControllerAdapter
    {
        #region Private fields
        
        private readonly ManualResetEvent _stopRequestEvent;
        private readonly List<EltraDevice> _devices;
        private readonly List<Task> _runningTasks;

        #endregion

        #region Constructors

        public ParameterControllerAdapter(string url, Channel session)
            :base(url, session)
        {
            _stopRequestEvent = new ManualResetEvent(false);
            _runningTasks = new List<Task>();

            _devices = new List<EltraDevice>();
        }
        
        #endregion

        #region Events handling

        private async void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            const int MaxLogValueLength = 8;
            string logValue = e.Parameter.ActualValue.Value;

            if(logValue.Length> MaxLogValueLength)
            {
                logValue = logValue.Substring(0, MaxLogValueLength-1);
            }

            MsgLogger.WriteLine($"changed: {e.Parameter.UniqueId}, new value = '{logValue}'");

            await UpdateParameterValue(e.Parameter.Device, e.Parameter);
        }

        #endregion

        #region Methods

        public async Task<Parameter> GetParameter(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            Parameter result = null;

            try
            {
                MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = sessionUuid;
                query["nodeId"] = $"{nodeId}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/get", query);

                var json = await Transporter.Get(url);

                var parameter = JsonConvert.DeserializeObject<Parameter>(json);

                if (parameter != null)
                {
                    result = parameter;

                    MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameter", e);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(EltraDevice device, Parameter parameter)
        {
            ParameterValue result = null;

            if (device != null && device.Identification!=null && parameter!=null)
            {
                result = await GetParameterValue(device.ChannelId, device.NodeId, parameter.Index, parameter.SubIndex);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(string sessionUuid, int nodeId, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            try
            {
                MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = sessionUuid;
                query["nodeId"] = $"{nodeId}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/value", query);

                var json = await Transporter.Get(url);

                var parameterValue = JsonConvert.DeserializeObject<ParameterValue>(json);

                if (parameterValue != null)
                {
                    result = parameterValue;

                    MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterValue", e);
            }

            return result;
        }

        private async Task StartUpdate(EltraDevice device)
        {
            if (await UpdateParameters(device))
            {
                RegisterEvents(device);

                Wait();
            
                UnregisterEvents(device);
            }
        }

        public void RegisterDevice(EltraDevice device)
        {
            if(!_devices.Contains(device))
            {
                if (ShouldRun())
                {
                    var task = Task.Run(async () =>
                    {
                        await StartUpdate(device);
                    });

                    _runningTasks.Add(task);
                }

                _devices.Add(device);
            }
        }

        private bool ShouldRun()
        {
            return !_stopRequestEvent.WaitOne(0);
        }

        private bool Wait(int timeout = int.MaxValue)
        {
            return _stopRequestEvent.WaitOne(timeout);
        }

        public override bool Stop()
        {
            if (_runningTasks.Count > 0)
            {
                _stopRequestEvent.Set();

                foreach (var task in _runningTasks)
                {
                    task.Wait();
                }
            }

            return base.Stop();
        }

        private async Task<bool> UpdateParameter(EltraDevice device, Parameter parameter)
        {
            bool result = false;

            try
            {
                var parameterUpdate = new ParameterUpdate
                {
                    ChannelId = Channel.Id,
                    NodeId = device.NodeId,
                    Parameter = parameter
                };

                var path = $"api/parameter/update";

                var json = JsonConvert.SerializeObject(parameterUpdate);

                var response = await Transporter.Put(Url, path, json);

                if(response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - UpdateParameter", $"update parameter '{parameter.UniqueId}' failed, reason = {response.StatusCode}");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - UpdateParameter", $"update parameter '{parameter.UniqueId}' failed, reason = no response");
                }
            }
            catch (Exception)
            {
                result = false;
            }
            
            return result;
        }

        private async Task<bool> UpdateParameterValue(EltraDevice device, Parameter parameter)
        {
            bool result = false;

            try
            {
                var parameterUpdate = new ParameterValueUpdate
                {
                    ChannelId = Channel.Id,
                    NodeId = device.NodeId,
                    ParameterValue = parameter.ActualValue,
                    Index = parameter.Index,
                    SubIndex = parameter.SubIndex
                };

                var path = $"api/parameter/value";

                var json = JsonConvert.SerializeObject(parameterUpdate);

                var response = await Transporter.Put(Url, path, json);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private async Task<bool> UpdateParameters(EltraDevice device)
        {
            bool result = true;
            var parameters = device?.ObjectDictionary?.Parameters;
            int minTimeout = 10;

            if (parameters != null)
            { 
                foreach (var parameter in parameters)
                {
                    if (!ShouldRun())
                    {
                        break;
                    }

                    if (parameter is Parameter parameterEntry && parameterEntry.ActualValue.IsValid)
                    {
                        var parameterValue = await GetParameterValue(device, parameterEntry);

                        if(parameterValue == null)
                        {
                            result = await UpdateParameter(device, parameterEntry);
                        }
                        else
                        {
                            result = parameterEntry.SetValue(parameterValue);
                        }
                        
                        await Task.Delay(minTimeout);
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                            foreach (var subParameter in subParameters)
                            {
                                if (!ShouldRun())
                                {
                                    break;
                                }

                                if (subParameter is Parameter subParameterEntry)
                                {
                                    if(subParameterEntry.ActualValue.IsValid)
                                    {

                                    }

                                    var parameterValue = await GetParameterValue(device, subParameterEntry);

                                    if (parameterValue == null)
                                    {
                                        result = await UpdateParameter(device, subParameterEntry);
                                    }
                                    else
                                    {
                                        result = subParameterEntry.SetValue(parameterValue);
                                    }

                                    await Task.Delay(minTimeout);
                                }
                            }
                    }
                }
            }

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(string callerId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            try
            {
                MsgLogger.WriteLine($"get parameter history, device serial number={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = callerId;
                query["nodeId"] = $"{nodeId}";
                query["uniqueId"] = $"{uniqueId}";
                query["from"] = $"{from}";
                query["to"] = $"{to}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/history", query);

                var json = await Transporter.Get(url);

                var parameterValues = JsonConvert.DeserializeObject<List<ParameterValue>>(json);

                if (parameterValues != null)
                {
                    result = parameterValues;

                    MsgLogger.WriteLine($"get parameter history, device serial number={nodeId} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterHistory", e);
            }

            return result;
        }

        public async Task<ParameterValueHistoryStatistics> GetParameterHistoryStatistics(string callerId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            ParameterValueHistoryStatistics result = null;

            try
            {
                MsgLogger.WriteLine($"get parameter history statistics, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = callerId;
                query["nodeId"] = $"{nodeId}";
                query["uniqueId"] = $"{uniqueId}";
                query["from"] = $"{from}";
                query["to"] = $"{to}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/history-statistics", query);

                var json = await Transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var historyStatistics = JsonConvert.DeserializeObject<ParameterValueHistoryStatistics>(json);

                    if (historyStatistics != null)
                    {
                        result = historyStatistics;

                        MsgLogger.WriteLine($"get parameter history statistics, device serial number={nodeId} - success");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - GetParameterHistoryStatistics", $"get parameter history statistics, device serial number={nodeId} - failed");
                }                
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterHistoryStatistics", e);
            }

            return result;
        }

        private void RegisterEvents(EltraDevice device)
        {
            var parameters = device?.ObjectDictionary?.Parameters;

            UnregisterEvents(device);

            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    if (parameter is Parameter parameterEntry)
                    {
                        parameterEntry.ParameterChanged += OnParameterChanged;
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                            foreach (var subParameter in subParameters)
                            {
                                if (subParameter is Parameter subParameterEntry)
                                {
                                    subParameterEntry.ParameterChanged += OnParameterChanged;
                                }
                            }
                    }
                }
        }

        private void UnregisterEvents(EltraDevice device)
        {
            var parameters = device?.ObjectDictionary?.Parameters;

            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    if (parameter is Parameter parameterEntry)
                    {
                        parameterEntry.ParameterChanged -= OnParameterChanged;
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                            foreach (var subParameter in subParameters)
                            {
                                if (subParameter is Parameter subParameterEntry)
                                {
                                    subParameterEntry.ParameterChanged -= OnParameterChanged;
                                }
                            }
                    }
                }
        }

        #endregion
    }
}
