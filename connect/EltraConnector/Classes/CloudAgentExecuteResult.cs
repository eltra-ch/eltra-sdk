using System.Collections.Generic;

using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.Classes
{
    public class ExecuteResult
    {
        #region Private fields

        private List<DeviceCommandParameter> _parameters;

        #endregion

        #region Properties

        public bool Result { get; set; }
        public uint ErrorCode { get; set; }

        public List<DeviceCommandParameter> Parameters => _parameters ?? (_parameters = new List<DeviceCommandParameter>());

        #endregion

        #region Methods

        public bool GetParameterValue<T>(string parameterName, ref T parameterValue)
        {
            bool result = false;

            if (Result)
            {
                var parameter = FindParameterByName(Parameters, parameterName);

                if (parameter != null)
                {
                    result = parameter.GetValue(ref parameterValue);
                }
            }

            return result;
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

        #endregion
    }
}
