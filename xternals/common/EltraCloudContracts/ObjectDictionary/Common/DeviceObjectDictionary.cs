using System;
using System.Collections.Generic;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.Logger;

namespace EltraCloudContracts.ObjectDictionary.Common
{
    public abstract class DeviceObjectDictionary
    {
        #region Private fields

        private List<ParameterBase> _parameters;
        private EltraDevice _device;
        
        #endregion

        #region Constructors

        public DeviceObjectDictionary(EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        public List<ParameterBase> Parameters
        {
            get => _parameters ?? (_parameters = new List<ParameterBase>());
            private set => _parameters = value; 
        }

        protected EltraDevice Device => _device;

        #endregion
        
        #region Private fields

        private Dd _xdd;

        #endregion

        #region Methods

        protected abstract bool CreateDeviceDescription();
        
        public virtual bool Open()
        {
            bool result = false;

            try
            {
                if(CreateDeviceDescription())
                {
                    if (_xdd.Parse())
                    {
                        SetParameters(_xdd.Parameters);
                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Open", "Parsing device description failed!");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Open", "Create device description failed!");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Open", e);

                result = false;
            }

            return result;
        }

        
        protected void SetParameters(List<ParameterBase> parameters)
        {
            Parameters = parameters;
        }

        protected void SetDeviceDescription(Dd xdd)
        {
            _xdd = xdd;
        }

        protected Dd GetDeviceDescription()
        {
            return _xdd;
        }
        
        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter is Parameter param && param.Index == index && param.SubIndex == subIndex)
                {
                    result = param;
                    break;
                }

                if (parameter is StructuredParameter sp && sp.Index == index)
                {
                    result = sp.SearchParameter(index, subIndex);

                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public ParameterBase SearchParameter(string uniqueId)
        {
            ParameterBase result = null;

            foreach (var parameter in Parameters)
            {
                if (parameter.UniqueId == uniqueId)
                {
                    result = parameter;
                    break;
                }

                if (parameter is StructuredParameter sp)
                {
                    result = sp.SearchParameter(uniqueId);

                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        #endregion


    }
}
