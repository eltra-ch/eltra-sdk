using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCommon.Contracts.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Contracts.Channels;
using EltraCommon.Threads;
using EltraConnector.Events;

#pragma warning disable 1591

namespace EltraConnector.Master.Device.ParameterConnection
{
    public class ParameterConnectionManager : EltraThread
    {
        #region Private fields

        private readonly MasterDevice _device;
        private readonly ParameterUpdater _parameterUpdater;
        private readonly object _lock = new object();

        private readonly List<RegisteredParameter> _registerParametersQueue;
        private readonly List<RegisteredParameter> _unregisterParametersQueue;

        private static readonly object RegisterParametersQueueLock = new object();
        private static readonly object UnregisterParametersQueueLock = new object();
        
        #endregion

        #region Constructors

        public ParameterConnectionManager(MasterDevice device, ParameterUpdatePriority priority = ParameterUpdatePriority.Medium)
        {
            Priority = priority;

            _parameterUpdater = new ParameterUpdater(device, priority);
            
            _registerParametersQueue = new List<RegisteredParameter>();
            _unregisterParametersQueue = new List<RegisteredParameter>();
           
            _device = device;

            RegisterEvents();
        }

        #endregion

        #region Properties

        public ParameterUpdatePriority Priority { get; }

        #endregion

        #region Events handling

        private void OnRemoteChannelStatusChanged(object sender, AgentChannelStatusChangedEventArgs e)
        {
            if (e.Status == ChannelStatus.Offline)
            {
                SourceChannelGoingOffline(e.Id);
            }
        }

        #endregion

        #region Methods

        public bool RegisterParameter(string source, ushort index, byte subIndex, ParameterUpdatePriority priority)
        {
            bool result = false;
            var objectDictionary = _device?.ObjectDictionary;
            var parameter = objectDictionary?.SearchParameter(index, subIndex);

            if (parameter != null)
            {
                var registeredParameter = new RegisteredParameter { Source = source, Parameter = parameter, Priority = priority };

                MsgLogger.WriteFlow($"{GetType().Name} - RegisterParameter", $"register parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, add to queue ...");

                lock (RegisterParametersQueueLock)
                {
                    _registerParametersQueue.Add(registeredParameter);
                    result = true;
                }

                MsgLogger.WriteFlow($"{GetType().Name} - RegisterParameter", $"register parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, added to queue");
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - RegisterParameter", $"parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, not found");
            }

            return result;
        }
        
        public bool UnregisterParameter(string source, ushort index, byte subIndex, ParameterUpdatePriority priority)
        {
            bool result = false;
            var objectDictionary = _device?.ObjectDictionary;
            var parameter = objectDictionary?.SearchParameter(index, subIndex);

            if (parameter != null)
            {
                var registeredParameter = new RegisteredParameter { Source = source, Parameter = parameter, Priority = priority };

                MsgLogger.WriteFlow($"{GetType().Name} - UnregisterParameter", $"un-register parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, add to queue ...");

                lock (UnregisterParametersQueueLock)
                {
                    _unregisterParametersQueue.Add(registeredParameter);
                    result = true;
                }

                MsgLogger.WriteFlow($"{GetType().Name} - UnregisterParameter", $"un-register parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, added to queue");
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - UnregisterParameter", $"parameter index = 0x{index:X4}, subindex = 0x{subIndex:X4}, not found");
            }

            return result;
        }
        
        public bool ReadParameter(Parameter parameter)
        {
            var parameterUpdater = _parameterUpdater;

            var result = parameterUpdater.ReadParameter(parameter);

            return result;
        }
        
        public bool WriteParameter(Parameter parameter)
        {
            bool result = false;
            var communication = _device?.Communication;

            lock (_lock)
            {
                if (parameter != null)
                {
                    if (parameter.Flags.Access == AccessMode.WriteOnly ||
                        parameter.Flags.Access == AccessMode.ReadWrite)
                    {
                        if (parameter.GetValue(out byte[] data))
                        {
                            MsgLogger.WriteFlow($"{GetType().Name} - WriteParameter", $"Write parameter, parameter='{parameter.UniqueId}', {data}");

                            if (communication.SetObject(parameter.Index, parameter.SubIndex, data))
                            {
                                result = true;
                            }
                            else
                            {
                                MsgLogger.WriteError($"{GetType().Name} - WriteParameter", $"Set value failed, parameter='{parameter.UniqueId}'");
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - WriteParameter", $"Get value failed, parameter='{parameter.UniqueId}'");
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - WriteParameter", $"Write not possible, index={parameter.Index}, subindex={parameter.SubIndex}, access flag={parameter.Flags.Access}");

                        result = false;
                    }
                }
            }

            return result;
        }
        
        private void RegisterEvents()
        {
            var agent = _device?.CloudAgent;

            if (agent != null)
            {
                agent.RemoteChannelStatusChanged += OnRemoteChannelStatusChanged;
            }
        }

        protected override async Task Execute()
        {
            const int minimalWaitTime = 10;

            try
            {
                _parameterUpdater.Start();

                while (ShouldRun())
                {
                    ProcessQueueRequests();

                    await Task.Delay(minimalWaitTime);
                }

                _parameterUpdater.Stop();
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Execute", e);
            }
           
            SetStopped();
        }
        
        private void ProcessQueueRequests()
        {
            ProcessUnregisterQueue();
            
            ProcessRegisterQueue();
        }

        private void ProcessRegisterQueue()
        {
            var parameterUpdater = _parameterUpdater;

            lock (_registerParametersQueue)
            {
                var queue = _registerParametersQueue.ToArray();

                foreach (var parameter in queue)
                {
                    if (!parameterUpdater.Register(parameter))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ProcessQueueRequests", "Error processing register request!");
                    }

                    _registerParametersQueue.Remove(parameter);
                }
            }
        }

        private void ProcessUnregisterQueue()
        {
            var parameterUpdater = _parameterUpdater;

            lock (_unregisterParametersQueue)
            {
                var queue = _unregisterParametersQueue.ToArray();

                foreach (var parameter in queue)
                {
                    if (!parameterUpdater.Unregister(parameter))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ProcessQueueRequests", "Error processing unregister request!");
                    }

                    _unregisterParametersQueue.Remove(parameter);
                }
            }
        }

        private void SourceChannelGoingOffline(string source)
        {
            var parameters = _parameterUpdater.GetParametersFromSource(source);

            MsgLogger.WriteFlow($"{GetType().Name} - SourceChannelGoingOffline", $"Source {source} is going offline ...");

            lock (UnregisterParametersQueueLock)
            {
                foreach (var parameter in parameters)
                {
                    _unregisterParametersQueue.Add(parameter);
                }
            }

            MsgLogger.WriteFlow($"{GetType().Name} - SourceChannelGoingOffline", $"Source {source} unregister queue ready!");
        }
        
        #endregion
    }
}
