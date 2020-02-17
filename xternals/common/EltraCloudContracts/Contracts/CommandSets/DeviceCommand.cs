using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.Contracts.CommandSets
{
    [DataContract]
    public class DeviceCommand
    {
        #region Private fields
        
        private List<DeviceCommandParameter> _parameters;

        #endregion

        #region Constructors

        public DeviceCommand()
        {
        }

        public DeviceCommand(EltraDevice device)
        {
            Device = device;
        }

        #endregion

        #region Properties

        [DataMember]
        public string Uuid { get; set; }
        
        [IgnoreDataMember]
        public EltraDevice Device { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ExecCommandStatus Status { get; set; }

        [DataMember]
        public List<DeviceCommandParameter> Parameters
        {
            get => _parameters ?? (_parameters = new List<DeviceCommandParameter>());
            private set => _parameters = value;
        }

        #endregion

        #region Methods

        public virtual bool Execute(string sourceUuid)
        {
            return false;
        }

        public bool AddParameter(DeviceCommandParameter parameter)
        {
            bool result = false;

            if (!ParameterExists(parameter))
            {
                Parameters.Add(parameter);
                result = true;
            }

            return result;
        }

        public bool AddParameter(string parameterName, TypeCode typeCode, ParameterType parameterType = ParameterType.In)
        {
            bool result = false;
            var parameter = new DeviceCommandParameter
                {Name = parameterName, Type = parameterType, DataType = new DataType {Type = typeCode}};

            if (!ParameterExists(parameter))
            {
                Parameters.Add(parameter);
                result = true;
            }

            return result;
        }

        public bool SetParameterValue<T>(string parameterName, T parameterValue, ParameterType parameterType = ParameterType.In)
        {
            bool result = false;

            var parameter = FindParameterByName(Parameters, parameterName);

            if (parameter != null)
            {
                result = parameter.SetValue(parameterValue);
            }
            else
            {
                parameter = new DeviceCommandParameter { Name = parameterName };

                if (parameter.SetValue(parameterValue))
                {
                    result = AddParameter(parameter);
                }
            }

            return result;
        }

        public bool ParameterExists(DeviceCommandParameter parameter)
        {
            return FindParameterByName(Parameters, parameter.Name) != null;
        }

        private DeviceCommandParameter FindParameterByName(List<DeviceCommandParameter> parameters, string name)
        {
            DeviceCommandParameter result = null;

            foreach (var parameter in parameters)
            {
                if (parameter.Name.ToLower() == name.ToLower())
                {
                    result = parameter;
                    break;
                }
            }

            return result;
        }

        public DeviceCommandParameter GetParameter(string parameterName)
        {
            DeviceCommandParameter result = null;

            var parameter = FindParameterByName(Parameters, parameterName);

            if (parameter != null)
            {
                result = parameter;
            }
            
            return result;
        }

        public bool GetParameterValue<T>(string parameterName, ref T parameterValue)
        {
            bool result = false;

            var parameter = FindParameterByName(Parameters, parameterName);

            if (parameter != null)
            {
                result = parameter.GetValue(ref parameterValue);
            }

            return result;
        }
        
        public void Sync(DeviceCommand command)
        {
            try
            {
                if (command != null)
                {
                    if (command.Device != null)
                    {
                        Device = command.Device;
                    }

                    Parameters = command.Parameters;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceCommand - Sync", e);
            }
        }

        public bool SetParameterDataType(string parameterName, DataType dataType)
        {
            bool result = false;

            var parameter = FindParameterByName(Parameters, parameterName);

            if (parameter != null)
            {
                result = parameter.SetDataType(dataType);
            }

            return result;
        }

        public bool SetParameterDataType(string parameterName, TypeCode typeCode)
        {
            bool result = false;

            var parameter = FindParameterByName(Parameters, parameterName);

            if (parameter != null)
            {
                result = parameter.SetDataType(typeCode);
            }

            return result;
        }

        public bool GetParameterDataType(string parameterName, out TypeCode typeCode)
        {
            bool result = false;
            var parameter = FindParameterByName(Parameters, parameterName);

            typeCode = TypeCode.Object;

            if (parameter != null)
            {
                result = parameter.GetDataType(out typeCode);
            }

            return result;
        }

        public virtual DeviceCommand Clone()
        {
            Clone(out DeviceCommand result);
            
            return result;
        }

        protected bool Clone<T>(out T clone)
        {
            bool result = false;
            
            clone = default;

            try
            {
                var serializer = new DataContractSerializer(typeof(T));
            
                using (Stream memoryStream = new MemoryStream())
                {
                    serializer.WriteObject(memoryStream, this);

                    memoryStream.Position = 0;

                    var deserializer = new DataContractSerializer(typeof(T));

                    clone = (T)deserializer.ReadObject(memoryStream);

                    if (clone is DeviceCommand deviceCommand)
                    {
                        deviceCommand.Device = Device;
                    }

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("DeviceCommand - Clone", e);    
            }
            
            return result;
        }

        #endregion
    }
}
