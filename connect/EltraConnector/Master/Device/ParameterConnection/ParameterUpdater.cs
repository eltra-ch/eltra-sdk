using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCommon.Contracts.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Threads;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraConnector.Master.Device.ParameterConnection
{
    class ParameterUpdater : EltraThread
    {
        #region Private fields

        private static readonly object RegisteredParametersLock = new object();
        private static readonly object ParameterReadLock = new object();

        private readonly Dictionary<string, RegisteredParameter> _registeredParameters;
        private readonly MasterDevice _device;

        #endregion

        #region Constructors

        public ParameterUpdater(MasterDevice device, ParameterUpdatePriority priority)
        {
            Priority = priority;

            _device = device;
            _registeredParameters = new Dictionary<string, RegisteredParameter>();
        }

        #endregion

        #region Properties

        public ParameterUpdatePriority Priority { get; }
        
        #endregion
        
        #region Methods

        protected override Task Execute()
        {
            const int minUpdateInterval = 10;
            
            while (ShouldRun())
            {
                try
                {
                    if (!ParametersUpdate())
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Execute", $"Parameter update failed!, priority: {Priority}");
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Execute", e);
                }
                
                Thread.Sleep(minUpdateInterval);
            }

            return Task.CompletedTask;
        }

        public bool ReadParameter(Parameter parameter)
        {
            bool result = false;
            var communication = _device?.Communication;

            lock (ParameterReadLock)
            {
                if (parameter != null)
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - ReadParameter", $"Read Parameter '{parameter.UniqueId}'");

                    if (parameter.Flags.Access != AccessMode.WriteOnly)
                    {
                        if(parameter.DataType!=null)
                        {
                            var data = new byte[parameter.DataType.SizeInBytes];

                            MsgLogger.WriteDebug($"{GetType().Name} - ReadParameter", $"Get Object '0x{parameter.Index:X4} 0x{parameter.SubIndex:X4}', data size={parameter.DataType.SizeInBytes}");

                            if (communication != null && communication.GetObject(parameter.Index, parameter.SubIndex, ref data))
                            {
                                result = parameter.SetValue(data);
                            }
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ReadParameter", $"Parameter '{parameter.UniqueId}' Data Type not defined!");
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - ReadParameter", $"Parameter '{parameter.UniqueId}' cannot be read, access flag {parameter.Flags.Access}!");

                        result = false;
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - ReadParameter", "Parameter undefined!");
                }
            }

            MsgLogger.WriteDebug($"{GetType().Name} - ReadParameter", $"Read Parameter '{parameter.UniqueId}' - result = '{result}'");

            return result;
        }

        private bool ParametersUpdate()
        {
            bool result = true;
            
            lock (RegisteredParametersLock)
            {
                if (_registeredParameters.Count > 0)
                {
                    foreach (var registeredParameter in _registeredParameters.Values)
                    {
                        if (!ShouldRun())
                        {
                            break;
                        }

                        if (registeredParameter.ElapsedTime >= _device.GetUpdateInterval(registeredParameter.Priority))
                        {
                            var parameter = registeredParameter.Parameter;

                            if (parameter is Parameter parameterEntry)
                            {
                                MsgLogger.WriteDebug($"{GetType().Name} - ParametersUpdate", $"Update Parameter '{parameterEntry.UniqueId}'");

                                result = ReadParameter(parameterEntry);

                                if(!result)
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - ParametersUpdate", $"Read Parameter '{parameterEntry.UniqueId}' failed!");
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
                                            result = ReadParameter(subParameterEntry);

                                            if (!result)
                                            {
                                                MsgLogger.WriteError($"{GetType().Name} - ParametersUpdate", $"Read Parameter '{subParameterEntry.UniqueId}' failed!");
                                            }
                                        }
                                    }
                                }
                            }

                            registeredParameter.Restart();
                        }
                    }
                }
            }

            return result;
        }

        public bool Register(RegisteredParameter registeredParameter)
        {
            bool result = false;

            MsgLogger.WriteFlow($"{GetType().Name} - Register", $"Register parameter source = '{registeredParameter.Key}', index = {registeredParameter.Parameter.Index:X4}, subindex = {(registeredParameter.Parameter as XddParameter).SubIndex:X4}");

            try
            {
                lock (RegisteredParametersLock)
                {
                    if (!_registeredParameters.ContainsKey(registeredParameter.Key))
                    {
                        result = AddToRegisteredParameters(registeredParameter);
                    }
                    else
                    {
                        result = UpdateParameterRefCount(registeredParameter);
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Register", e);
            }

            MsgLogger.WriteDebug($"{GetType().Name} - Register", $"Register Parameter '{registeredParameter.Key}' - OK");

            return result;
        }

        private bool UpdateParameterRefCount(RegisteredParameter registeredParameter)
        {
            bool result = false;
            var parameter = _registeredParameters[registeredParameter.Key];

            if (parameter != null)
            {
                parameter.RefCount++;

                MsgLogger.WriteLine($"{GetType().Name} - Register", $"(+)register parameter key='{registeredParameter.Key}', refCount = {parameter.RefCount}");

                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - Register", $"(+)register parameter key='{registeredParameter.Key}' failed, parameter empty!");
            }

            return result;
        }

        private bool AddToRegisteredParameters(RegisteredParameter registeredParameter)
        {
            bool result = false;

            if (_registeredParameters != null)
            {
                _registeredParameters.Add(registeredParameter.Key, registeredParameter);

                registeredParameter.RefCount++;

                MsgLogger.WriteLine($"{GetType().Name} - AddToRegisteredParameters", $"(+)register parameter key='{registeredParameter.Key}'");

                result = true;
            }

            return result;
        }

        public bool Unregister(RegisteredParameter registeredParameter)
        {
            bool result = false;

            MsgLogger.WriteDebug($"{GetType().Name} - Unregister", $"Unregister Parameter '{registeredParameter.Key}', index = {registeredParameter.Parameter.Index:X4}, subindex = {(registeredParameter.Parameter as XddParameter).SubIndex:X4}");

            lock (RegisteredParametersLock)
            {
                if (_registeredParameters.ContainsKey(registeredParameter.Key))
                {
                    var parameter = _registeredParameters[registeredParameter.Key];

                    if (parameter != null)
                    {
                        parameter.RefCount--;

                        if (parameter.RefCount == 0)
                        {
                            result = _registeredParameters.Remove(registeredParameter.Key);

                            MsgLogger.WriteLine($"{GetType().Name} - Unregister", $"(-)unregister parameter key='{registeredParameter.Key}', result={result}");
                        }
                        else
                        {
                            MsgLogger.WriteLine($"{GetType().Name} - Unregister", $"(-)unregister parameter key='{registeredParameter.Key}', refCount = {parameter.RefCount}");

                            result = true;
                        }
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Unregister", $"(-)unregister parameter key='{registeredParameter.Key}' failed, parameter empty!");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Unregister", $"(-)unregister parameter key='{registeredParameter.Key}' failed, parametere not found!");
                }
            }

            MsgLogger.WriteDebug($"{GetType().Name} - Unregister", $"Unregister Parameter '{registeredParameter.Key}' - OK");

            return result;
        }
        
        public List<RegisteredParameter> GetParametersFromSource(string source)
        {
            var result = new List<RegisteredParameter>();

            MsgLogger.WriteFlow($"{GetType().Name} - GetParametersFromSource", $"GetParametersFromSource enter ...");

            lock (RegisteredParametersLock)
            {
                if (_registeredParameters.Count > 0)
                {
                    foreach (var registeredParameter in _registeredParameters.Values)
                    {
                        if (registeredParameter.Source == source)
                        {
                            result.Add(registeredParameter);
                        }
                    }
                }
            }

            MsgLogger.WriteFlow($"{GetType().Name} - GetParametersFromSource", $"GetParametersFromSource leave");

            return result;
        }

        #endregion
    }
}
