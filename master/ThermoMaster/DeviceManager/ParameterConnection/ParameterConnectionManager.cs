using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using ThermoMaster.DeviceManager.Device;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Threads;

namespace ThermoMaster.DeviceManager.ParameterConnection
{
    class ParameterConnectionManager : EltraThread
    {
        #region Private fields

        private readonly ThermoDeviceBase _device;
        private readonly ParameterUpdater _parameterUpdater;

        private readonly List<RegisteredParameter> _registerParametersQueue;
        private readonly List<RegisteredParameter> _unregisterParametersQueue;

        private static readonly object RegisterParametersQueueLock = new object();
        private static readonly object UnregisterParametersQueueLock = new object();
        
        #endregion

        #region Constructors

        public ParameterConnectionManager(ThermoDeviceBase device, ParameterUpdatePriority priority = ParameterUpdatePriority.Medium)
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

        private void OnRemoteSessionStatusChanged(object sender, EltraConnector.Events.SessionStatusChangedEventArgs e)
        {
            if (e.Status == SessionStatus.Offline)
            {
                SourceSessionGoingOffline(e.Uuid);
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

                lock(RegisterParametersQueueLock)
                {
                    _registerParametersQueue.Add(registeredParameter);
                    result = true;
                }
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

                lock(UnregisterParametersQueueLock)
                {
                    _unregisterParametersQueue.Add(registeredParameter);
                    result = true;
                }
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

            lock (this)
            {
                if (parameter != null)
                {
                    if (parameter.Flags.Access == AccessMode.WriteOnly ||
                        parameter.Flags.Access == AccessMode.ReadWrite)
                    {
                        if (parameter.GetValue(out byte[] data))
                        {
                            MsgLogger.WriteFlow( $"Write parameter, parameter='{parameter.UniqueId}', {data}");

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
                agent.RemoteSessionStatusChanged += OnRemoteSessionStatusChanged;
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
        
        private void SourceSessionGoingOffline(string source)
        {
            var parameters = _parameterUpdater.GetParametersFromSource(source);

            lock (UnregisterParametersQueueLock)
            {
                foreach (var parameter in parameters)
                {
                    _unregisterParametersQueue.Add(parameter);
                }
            }
        }
        
        #endregion
    }
}
