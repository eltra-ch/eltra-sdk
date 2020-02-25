using EposMaster.DeviceManager.Device.Epos4.Commands;

namespace EposMaster.DeviceManager.Device
{
    sealed class Epos2Device : EposDevice
    {
        #region Constructors

        public Epos2Device(string deviceDescriptionFile)
            : base("EPOS2", deviceDescriptionFile)
        {
            CreateCommandSet();
        }

        #endregion

        #region Methods

        private void CreateCommandSet()
        {
            AddCommand(new GetEnableStateCommand(this));
            AddCommand(new GetDisableStateCommand(this));

            AddCommand(new ClearFaultCommand(this));
        }

        #endregion
    }
}
