using System;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events
{
    public class ParameterChangedEventArgs : EventArgs
    {
        public ParameterChangedEventArgs(Parameter parameter, ParameterValue oldValue, ParameterValue newValue)
        {
            Parameter = parameter;

            OldValue = oldValue;
            NewValue = newValue;
        }
        
        public Parameter Parameter { get; }
        public ParameterValue OldValue { get; }
        public ParameterValue NewValue { get; }
    }
}
