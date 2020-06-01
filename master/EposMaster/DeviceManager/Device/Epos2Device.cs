using EposMaster.DeviceManager.Device.Epos4.Commands;

namespace EposMaster.DeviceManager.Device
{
    sealed class Epos2Device : EposDevice
    {
        #region Constructors

        public Epos2Device(string deviceDescriptionFile, uint updateInterval, uint timeout)
            : base("EPOS2", deviceDescriptionFile, updateInterval, timeout)
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
