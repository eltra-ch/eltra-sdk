using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EltraConnector.Controllers.Queue;
using EltraCommon.Helpers;
using EltraCommon.Contracts.Channels;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.History;
using EltraConnector.Controllers.Events;
using EltraCommon.Contracts.Users;
using EltraCommon.Transport;
using EltraCommon.Extensions;
using System.Globalization;

namespace EltraConnector.Controllers
{
    internal class ParameterControllerAdapter : CloudChannelControllerAdapter
    {
        #region Private fields
        
        private readonly ManualResetEvent _stopRequestEvent;
        private readonly List<EltraDevice> _devices;
        private readonly List<Task> _runningTasks;
        private readonly UserIdentity _identity;
        private readonly ParameterChangeQueue _parameterChangeQueue;

        #endregion

        #region Constructors

        public ParameterControllerAdapter(IHttpClient httpClient, UserIdentity identity, string url, Channel channel)
            :base(httpClient, url, channel)
        {
            _identity = identity;
            _stopRequestEvent = new ManualResetEvent(false);
            _runningTasks = new List<Task>();
            _parameterChangeQueue = new ParameterChangeQueue();

            _devices = new List<EltraDevice>();
        }

        #endregion

        #region Events

        public event EventHandler<ParameterUpdateEventArgs> ParametersUpdated;

        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            const string method = "OnParameterChanged";

            try
            {
                Parameter parameter = e.Parameter;

                if (parameter != null)
                {
                    var device = parameter.Device;
                    if (device != null)
                    {
                        if (parameter.Flags.Volatile != 0)
                        {
                            QueueParameterChanged(new ParameterChangeQueueItem(device.NodeId, parameter, e.NewValue, e.OldValue));
                        }
                        else
                        {
                            int maxWaitTime;

#if DEBUG
                            maxWaitTime = 60000;
#else
                            maxWaitTime = 3000;
#endif

                            var task = Task.Run(async () =>
                            {
                                await SetValue(parameter, e);
                            }).Wait(maxWaitTime);

                            if(!task)
                            {
                                MsgLogger.WriteError($"{GetType().Name} - {method}", $"SetValue {parameter.Index:X2}:{parameter.SubIndex:X2} failed! timeout!");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MsgLogger.Exception($"{GetType().Name} - OnParameterChanged", ex);
            }
        }

        private async Task SetValue(Parameter parameter, ParameterChangedEventArgs e)
        {
            const string method = "SetValue";

            if (!await parameter.SetParameterValue(e.NewValue))
            {
                //TODO parameter.SetValue(e.OldValue);

                MsgLogger.WriteError($"{GetType().Name} - {method}", $"SetParameterValue {parameter.Index:X2}:{parameter.SubIndex:X2} failed!");
            }
        }

        private void QueueParameterChanged(ParameterChangeQueueItem queueItem)
        {
            queueItem.WorkingTask = Task.Run(async () =>
            {
                MsgLogger.WriteLine($"{GetType().Name} - QueueParameterChanged", $"changed: {queueItem.UniqueId}, new value = '{CreateShortLogValue(queueItem.NewValue)}'");

                bool skipProcessing = _parameterChangeQueue.ShouldSkip(queueItem);

                if (!skipProcessing)
                {
                    if(!await queueItem.Update())
                    {
                        MsgLogger.WriteError($"{GetType().Name} - QueueParameterChanged", 
                            $"update failed: {queueItem.UniqueId}, new value = '{CreateShortLogValue(queueItem.NewValue)}'");
                    }
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - QueueParameterChanged", "Skip parameter change processing, queue already has item with higher timestamp");
                }                
            });

            _parameterChangeQueue.Add(queueItem);

            queueItem.WorkingTask.Wait();
        }

        private static string CreateShortLogValue(ParameterValue actualValue)
        {
            const int MaxLogValueLength = 8;
            string logValue = actualValue.Value;

            if (logValue.Length > MaxLogValueLength)
            {
                logValue = logValue.Substring(0, MaxLogValueLength - 1);
            }

            return logValue;
        }

        private void OnParametersUpdated(EltraDevice device, bool updateResult)
        {
            ParametersUpdated?.Invoke(this, new ParameterUpdateEventArgs() { Device = device, Result = updateResult });
        }

        #endregion

        #region Methods

        public async Task<Parameter> GetParameter(string channelId, int nodeId, ushort index, byte subIndex)
        {
            Parameter result = null;

            try
            {
                MsgLogger.WriteLine($"{GetType().Name} - GetParameter", $"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = channelId;
                query["nodeId"] = $"{nodeId}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/get", query);

                var json = await Transporter.Get(_identity, url);

                var parameter = json.TryDeserializeObject<Parameter>();

                if (parameter != null)
                {
                    result = parameter;

                    MsgLogger.WriteLine($"{GetType().Name} - GetParameter", $"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId} - success");
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

        public async Task<ParameterValue> GetParameterValue(string channelId, int nodeId, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            try
            {
                MsgLogger.WriteLine($"{GetType().Name} - GetParameterValue", $"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = channelId;
                query["nodeId"] = $"{nodeId}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/value", query);

                var json = await Transporter.Get(_identity, url);

                var parameterValue = json.TryDeserializeObject<ParameterValue>();

                if (parameterValue != null)
                {
                    result = parameterValue;

                    MsgLogger.WriteLine($"{GetType().Name} - GetParameterValue", $"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device node id={nodeId} - success");
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
            const string method = "StartUpdate";

            RegisterEvents(device);

            if (await UpdateParameters(device))
            {
                if (!Wait())
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", "timeout");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", "Update parameters failed!");
            }

            UnregisterEvents(device);
        }

        public bool RegisterDevice(EltraDevice device)
        {
            bool result = false;

            if(!_devices.Contains(device))
            {
                _devices.Add(device);

                device.ConnectorChanged += (o,e) =>
                {
                    StartUpdateAsync(device);
                };

                if(device.Status == DeviceStatus.Ready)
                {
                    StartUpdateAsync(device);
                }
                
                result = true;
            }

            return result;
        }

        private void StartUpdateAsync(EltraDevice device)
        {
            if (ShouldRun())
            {
                var task = Task.Run(async () =>
                {
                    await StartUpdate(device);
                });

                _runningTasks.Add(task);
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

        private async Task<bool> UpdateParameters(EltraDevice device)
        {
            bool result = true;
            var parameters = device?.ObjectDictionary?.Parameters;

            if (parameters != null)
            { 
                foreach (var parameter in parameters)
                {
                    if (!ShouldRun())
                    {
                        break;
                    }

                    if (parameter is Parameter parameterEntry)
                    {
                        var parameterValue = await GetParameterValue(device, parameterEntry);

                        if (parameterValue != null)
                        {
                            result = parameterEntry.SetValue(parameterValue);
                        }
                    }
                    else if (parameter is StructuredParameter structuredParameter)
                    {
                        var subParameters = structuredParameter.Parameters;
                        if (subParameters != null)
                        {
                            foreach (var subParameter in subParameters)
                            {
                                if (!ShouldRun())
                                {
                                    break;
                                }

                                if (subParameter is Parameter subParameterEntry)
                                {
                                    var parameterValue = await GetParameterValue(device, subParameterEntry);

                                    if (parameterValue != null)
                                    {
                                        result = subParameterEntry.SetValue(parameterValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            OnParametersUpdated(device, result);

            return result;
        }

        public async Task<List<ParameterValue>> GetParameterHistory(string callerId, int nodeId, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            try
            {
                MsgLogger.WriteLine($"{GetType().Name} - GetParameterHistory", $"get parameter history, device serial number={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = callerId;
                query["nodeId"] = $"{nodeId}";
                query["uniqueId"] = $"{uniqueId}";

                query["from"] = $"{from.ToString("o", CultureInfo.InvariantCulture)}";
                query["to"] = $"{to.ToString("o", CultureInfo.InvariantCulture)}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/history", query);

                var json = await Transporter.Get(_identity, url);

                var parameterValues = json.TryDeserializeObject<ParameterValueList>();

                if (parameterValues != null)
                {
                    result = parameterValues.Items;

                    MsgLogger.WriteLine($"{GetType().Name} - GetParameterHistory", $"get parameter history, device serial number={nodeId} - success");
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
                MsgLogger.WriteLine($"{GetType().Name} - GetParameterHistoryStatistics", $"get parameter history statistics, device node id={nodeId}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["callerId"] = callerId;
                query["nodeId"] = $"{nodeId}";
                query["uniqueId"] = $"{uniqueId}";

                query["from"] = $"{from.ToString("o", CultureInfo.InvariantCulture)}";
                query["to"] = $"{to.ToString("o", CultureInfo.InvariantCulture)}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/history-statistics", query);

                var json = await Transporter.Get(_identity, url);

                if (!string.IsNullOrEmpty(json))
                {
                    var historyStatistics = json.TryDeserializeObject<ParameterValueHistoryStatistics>();

                    if (historyStatistics != null)
                    {
                        result = historyStatistics;

                        MsgLogger.WriteLine($"{GetType().Name} - GetParameterHistoryStatistics", $"get parameter history statistics, device serial number={nodeId} - success");
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
