using System.Collections.Generic;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription
{
    public abstract class Dd
    {
        public List<ParameterBase> Parameters { get; set; }

        public string DataSource { get; set; }

        public abstract bool Parse();
    }
}
