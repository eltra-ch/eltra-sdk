using System.Collections.Generic;

using EltraCommon.Contracts.CommandSets;

namespace EltraConnector.Classes
{
    /// <summary>
    /// {ExecuteResult} class - result of command execution
    /// </summary>
    public class ExecuteResult
    {
        #region Private fields

        private List<DeviceCommandParameter> _parameters;

        #endregion

        #region Properties

        /// <summary>
        /// execution result
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// device error code
        /// </summary>
        public uint ErrorCode { get; set; }

        /// <summary>
        /// List of command {DeviceCommandParameter}
        /// </summary>
        public List<DeviceCommandParameter> Parameters => _parameters ?? (_parameters = new List<DeviceCommandParameter>());

        #endregion

        #region Methods

        /// <summary>
        /// GetParameterValue - get parameter value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterName">parameter name</param>
        /// <param name="parameterValue">parameter value</param>
        /// <returns></returns>
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
