using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes
{
    class XddDataTypeReference : DataTypeReference
    {
        public string UniqueId { get; set; }

        public string Name { get; set; }
        
        public override bool Parse()
        {
            return false;
        }
    }
}
