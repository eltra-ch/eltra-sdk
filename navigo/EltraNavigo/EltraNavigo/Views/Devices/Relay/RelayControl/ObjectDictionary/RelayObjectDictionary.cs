using EltraCloudContracts.ObjectDictionary.Common;

namespace EltraNavigo.Views.RelayControl.ObjectDictionary
{
    public class RelayObjectDictionary : DeviceObjectDictionary
    {
        #region Constructors

        public RelayObjectDictionary(EltraCloudContracts.Contracts.Devices.EltraDevice device)
            : base(device)
        {   
        }

        protected override bool CreateDeviceDescription()
        {
            return true;
        }

        #endregion

        #region Methods


        #endregion
    }
}
