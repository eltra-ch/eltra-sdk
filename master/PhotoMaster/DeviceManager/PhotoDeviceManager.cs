using EltraMaster.Device;
using PhotoMaster.DeviceManager.Device;
using PhotoMaster.DeviceManager.Wrapper;
using PhotoMaster.Settings;

namespace PhotoMaster.DeviceManager
{
    public class PhotoDeviceManager : MasterDeviceManager
    {
        public PhotoDeviceManager(MasterSettings settings)
        {
            EltraFsWebCamWrapper.Initialize();

            AddDevice(new PhotoDevice(settings));
        }

        protected override void Dispose(bool finalize)
        {
            base.Dispose(finalize);

            EltraFsWebCamWrapper.Release();
        }
    }
}
