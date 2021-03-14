using EltraConnector.Classes;
using System.Collections.Generic;

namespace EltraConnector.Agent.UserAgent.Cache
{
    class ParameterRegistrationCache
    {
        #region Private fields

        private static object _registeredParameterLocker = new object();
        
        private List<RegisteredParameter> _registeredParameters;

        #endregion

        #region Properties

        private List<RegisteredParameter> RegisteredParameters => _registeredParameters ?? (_registeredParameters = new List<RegisteredParameter>());

        #endregion

        #region Methods

        /// <summary>
        /// IsParameterRegistered
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="instanceCount"></param>
        /// <returns></returns>
        public bool IsParameterRegistered(string uniqueId, ushort index, byte subIndex, out int instanceCount)
        {
            bool result = false;

            instanceCount = 0;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId && parameter.InstanceCount > 0)
                    {
                        result = true;
                        break;
                    }
                }

                if(result == false)
                {
                    instanceCount = AddParameter(uniqueId, index, subIndex);
                }
            }

            return result;
        }

        public bool IsParameterRegistered(ushort index, byte subIndex)
        {
            bool result = false;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.Index == index && parameter.SubIndex == subIndex && parameter.InstanceCount > 0)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private int AddParameter(string uniqueId, ushort index, byte subIndex)
        {
            int result;

            if (FindParameterInternal(uniqueId, out var existingParameter))
            {
                existingParameter.InstanceCount++;

                result = existingParameter.InstanceCount;
            }
            else
            {   
                var parameter = new RegisteredParameter(uniqueId, index, subIndex, _registeredParameterLocker) { InstanceCount = 1 };

                RegisteredParameters.Add(parameter);

                result = parameter.InstanceCount;
            }

            return result;
        }

        public int AddParameter(RegisteredParameter registeredParameter)
        {
            int result = 0;

            if (FindParameter(registeredParameter.UniqueId, out var existingParameter))
            {
                existingParameter.InstanceCount++;

                result = existingParameter.InstanceCount;
            }
            else
            {
                lock (_registeredParameterLocker)
                {
                    registeredParameter.InstanceCount = 1;

                    RegisteredParameters.Add(registeredParameter);

                    result = registeredParameter.InstanceCount;
                }
            }

            return result;
        }

        public bool FindParameter(string uniqueId, out RegisteredParameter registeredParameter)
        {
            bool result = false;

            registeredParameter = null;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        registeredParameter = parameter;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public bool CanUnregister(string uniqueId, out RegisteredParameter registeredParameter)
        {
            bool result = false;

            registeredParameter = null;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        registeredParameter = parameter;
                        break;
                    }
                }

                if(registeredParameter != null)
                {
                    if(registeredParameter.InstanceCount == 1)
                    {
                        registeredParameter.Release();
                        result = true;
                    }
                }
            }

            return result;
        }

        public bool IncreaseCounter(string uniqueId, out RegisteredParameter registeredParameter)
        {
            bool result = false;

            registeredParameter = null;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        registeredParameter = parameter;
                        result = true;
                        break;
                    }
                }

                if(registeredParameter != null)
                {
                    registeredParameter.InstanceCount++;
                }
            }

            return result;
        }

        private bool FindParameterInternal(string uniqueId, out RegisteredParameter registeredParameter)
        {
            bool result = false;

            registeredParameter = null;

            foreach (var parameter in RegisteredParameters)
            {
                if (parameter.UniqueId == uniqueId)
                {
                    registeredParameter = parameter;
                    result = true;
                    break;
                }
            }

            return result;
        }

        public bool FindParameter(ushort index, byte subIndex, out RegisteredParameter registeredParameter)
        {
            bool result = false;

            registeredParameter = null;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.Index == index && parameter.SubIndex == subIndex)
                    {
                        registeredParameter = parameter;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public int RemoveParameter(string uniqueId)
        {
            int result = 0;

            lock (_registeredParameterLocker)
            {
                foreach (var parameter in RegisteredParameters)
                {
                    if (parameter.UniqueId == uniqueId)
                    {
                        if (parameter.InstanceCount > 0)
                        {
                            parameter.InstanceCount--;
                        }
                        
                        result = parameter.InstanceCount;

                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
