using EltraCloudContracts.Contracts.Users;
using EltraMaster;
using System.Threading.Tasks;
using ThermoMaster.DeviceManager;
using ThermoMaster.DeviceManager.Wrapper;
using ThermoMaster.Settings;

namespace ThermoMaster
{
    public class ThermoMaster : Master
    {
        private readonly MasterSettings _settings;

        #region Constructors

        public ThermoMaster(MasterSettings settings)
        {
            _settings = settings;
        }

        #endregion
        
        public async Task Start(string host, UserAuthData authData)
        {
            EltraBmp180Wrapper.Initialize();
            EltraDht22Wrapper.Initialize();
            EltraRelayWrapper.Initialize();
            EltraSds011Wrapper.Initialize();

            await Start(host, authData, new ThermoDeviceManager(_settings), _settings.UpdateInterval, _settings.Timeout);

            EltraBmp180Wrapper.Release();
            EltraRelayWrapper.Release();
            EltraDht22Wrapper.Release();
            EltraSds011Wrapper.Release();
        }
    }
}
