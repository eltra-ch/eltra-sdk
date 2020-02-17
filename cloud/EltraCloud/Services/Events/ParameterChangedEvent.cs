using EltraCloudContracts.Contracts.Parameters;
using System;

#pragma warning disable CS1591

namespace EltraCloud.Services.Events
{
    public class ParameterValueChangedEventArgs : EventArgs
    {
        public ParameterValueChangedEventArgs(ParameterUpdate parameterUpdate)
        {
            ParameterUpdate = parameterUpdate;
        }

        public ParameterUpdate ParameterUpdate { get; set; }
    }
}
