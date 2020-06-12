using PhotoMaster.Settings;
using EltraMaster.Device;

namespace PhotoMaster.DeviceManager.Device
{
    sealed class PhotoDevice : MasterDevice
    {
        #region Private fields

        private readonly MasterSettings _settings;
        
        #endregion

        #region Constructors

        public PhotoDevice(MasterSettings settings)
            : base("PHOTO", settings.Device.XddFile)
        {
            _settings = settings;
            
            Identification.SerialNumber = _settings.Device.SerialNumber;

            ExtendCommandSet();
        }

        #endregion

        #region Properties

        public MasterSettings Settings { get => _settings; }

        #endregion

        #region Methods

        private void ExtendCommandSet()
        {            
            //Object
            AddCommand(new Commands.TakePictureCommand(this));        
        }

        protected override void CreateCommunication()
        {
            var communication = new PhotoDeviceCommunication(this, Settings);

            Communication = communication;
        }

        #endregion
    }
}
