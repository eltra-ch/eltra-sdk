using EltraCloudContracts.Contracts.Users;
using EltraMaster;
using EposMaster.DeviceManager;
using EposMaster.Settings;
using System.Threading.Tasks;

namespace EposMaster
{
    public class EposMaster : Master
    {
        #region Constructors

        private readonly MasterSettings _settings;
                
        #endregion

        #region Constructors

        public EposMaster(MasterSettings settings)
        {
            _settings = settings;
        }

        #endregion

        #region Methods

        public async Task Start(string host, UserAuthData authData)
        {
            DeviceManager.VcsWrapper.Device.Init();

            await Start(host, authData, new EposDeviceManager(_settings), _settings.UpdateInterval, _settings.Timeout);

            DeviceManager.VcsWrapper.Device.Cleanup();
        }
            
        #endregion
    }
}
