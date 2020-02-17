using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes
{
    class Epos4DataTypeReference : DataTypeReference
    {
        public string UniqueId { get; set; }

        public string Name { get; set; }
        
        public override bool Parse()
        {
            return false;
        }
    }
}
