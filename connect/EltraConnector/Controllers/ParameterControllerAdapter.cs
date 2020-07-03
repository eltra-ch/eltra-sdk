using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using EltraConnector.Controllers.Base;

using EltraCommon.Helpers;

using EltraCommon.Contracts.Parameters;
using EltraCommon.Contracts.Sessions;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using Newtonsoft.Json;
using EltraCommon.Contracts.Devices;
using System.Globalization;

namespace EltraConnector.Controllers
{
    public class ParameterControllerAdapter : CloudSessionControllerAdapter
    {
        #region Private fields
        
        private readonly ManualResetEvent _stopRequestEvent;
        private ManualResetEvent _stopped;
        private readonly ManualResetEvent _running;
        private readonly List<EltraDevice> _devices;

        #endregion

        #region Constructors

        public ParameterControllerAdapter(string url, Session session)
            :base(url, session)
        {
            _stopRequestEvent = new ManualResetEvent(false);
            _stopped = new ManualResetEvent(true);
            _running = new ManualResetEvent(false);

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

            await UpdateParameter(e.Parameter.Device, e.Parameter);
        }

        #endregion

        #region Methods

        public async Task<Parameter> GetParameter(ulong serialNumber, ushort index, byte subIndex)
        {
            Parameter result = null;

            try
            {
                MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number=0x{serialNumber:X}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = Session.Uuid;
                query["serialNumber"] = $"{serialNumber}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/get", query);

                var json = await Transporter.Get(url);

                var parameter = JsonConvert.DeserializeObject<Parameter>(json);

                if (parameter != null)
                {
                    result = parameter;

                    MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number=0x{serialNumber:X} - success");
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
                var serialNumber = device.Identification.SerialNumber;

                result = await GetParameterValue(serialNumber, parameter.Index, parameter.SubIndex);
            }

            return result;
        }

        public async Task<ParameterValue> GetParameterValue(ulong serialNumber, ushort index, byte subIndex)
        {
            ParameterValue result = null;

            try
            {
                MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number=0x{serialNumber:X}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = Session.Uuid;
                query["serialNumber"] = $"{serialNumber}";
                query["index"] = $"{index}";
                query["subIndex"] = $"{subIndex}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/value", query);

                var json = await Transporter.Get(url);

                var parameterValue = JsonConvert.DeserializeObject<ParameterValue>(json);

                if (parameterValue != null)
                {
                    result = parameterValue;

                    MsgLogger.WriteLine($"get parameter, index=0x{index:X4}, subindex=0x{subIndex:X4}, device serial number=0x{serialNumber:X} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterValue", e);
            }

            return result;
        }

        private async void StartUpdate(EltraDevice device)
        {
            //int updateParametersInterval = 10;

            _running.Set();

            _stopped = new ManualResetEvent(false);

            if (await UpdateParameters(device))
            {
                RegisterEvents(device);

                Wait();
                /*while (ShouldRun())
                {
                    Thread.Sleep(updateParametersInterval);
                }*/

                UnregisterEvents(device);
            }

            _stopped.Set();
        }

        private bool IsStopped()
        {
            return _stopped.WaitOne(0);
        }

        public void RegisterDevice(EltraDevice device)
        {
            if(!_devices.Contains(device))
            { 
                _devices.Add(device);
            }

            if (IsStopped())
            {
                Task.Run(()=> 
                { 
                    StartUpdate(device); 
                });
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
            if (_running.WaitOne(0))
            {
                _stopRequestEvent.Set();

                _stopped.WaitOne();

                _running.Reset();
            }

            return base.Stop();
        }

        private async Task<bool> UpdateParameter(EltraDevice device, Parameter parameter)
        {
            bool result = true;

            try
            {
                var parameterUpdate = new ParameterUpdate
                {
                    SessionUuid = Session.Uuid,
                    SerialNumber = device.Identification.SerialNumber,
                    Parameter = parameter
                };

                var path = $"api/parameter/update";

                var json = JsonConvert.SerializeObject(parameterUpdate);

                await Transporter.Put(Url, path, json);
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

                                if (subParameter is Parameter subParameterEntry && subParameterEntry.ActualValue.IsValid)
                                {
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

        public async Task<List<ParameterValue>> GetParameterHistory(ulong serialNumber, string uniqueId, DateTime from, DateTime to)
        {
            var result = new List<ParameterValue>();

            try
            {
                MsgLogger.WriteLine($"get parameter history, device serial number=0x{serialNumber:X}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = Session.Uuid;
                query["serialNumber"] = $"{serialNumber}";
                query["uniqueId"] = $"{uniqueId}";
                query["from"] = $"{from}";
                query["to"] = $"{to}";

                var url = UrlHelper.BuildUrl(Url, "api/parameter/history", query);

                var json = await Transporter.Get(url);

                var parameterValues = JsonConvert.DeserializeObject<List<ParameterValue>>(json);

                if (parameterValues != null)
                {
                    result = parameterValues;

                    MsgLogger.WriteLine($"get parameter, device serial number=0x{serialNumber:X} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterHistory", e);
            }

            return result;
        }

        public async Task<List<ParameterUniqueIdValuePair>> GetParameterHistoryPair(ulong serialNumber, string uniqueId1, string uniqueId2, DateTime from, DateTime to)
        {
            var result = new List<ParameterUniqueIdValuePair>();

            try
            {
                MsgLogger.WriteLine($"get parameter history, device serial number=0x{serialNumber:X}");

                var query = HttpUtility.ParseQueryString(string.Empty);

                query["uuid"] = Session.Uuid;
                query["serialNumber"] = $"{serialNumber}";
                query["uniqueId1"] = $"{uniqueId1}";
                query["uniqueId2"] = $"{uniqueId2}";
                
                query["from"] = from.ToString("s", CultureInfo.InvariantCulture);
                query["to"] = to.ToString("s", CultureInfo.InvariantCulture);

                var url = UrlHelper.BuildUrl(Url, "api/parameter/pair-history", query);

                var json = await Transporter.Get(url);

                var parameterValues = JsonConvert.DeserializeObject<List<ParameterUniqueIdValuePair>>(json);

                if (parameterValues != null)
                {
                    result = parameterValues;

                    MsgLogger.WriteLine($"get parameter, device serial number=0x{serialNumber:X} - success");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetParameterHistoryPair", e);
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
