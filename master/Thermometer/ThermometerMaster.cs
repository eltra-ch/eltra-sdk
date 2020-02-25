using EltraCloudContracts.Contracts.Users;
using ThermoMaster.Settings;
using System.Threading.Tasks;
using Thermometer.DeviceManager.Device;
using EltraMaster;
using Thermometer.DeviceManager;

namespace ThermoMaster
{
    public class ThermometerMaster : Master
    {
        #region Constructors

        private readonly MasterSettings _settings;
        
        #endregion

        #region Constructors

        public ThermometerMaster(MasterSettings settings)
        {
            _settings = settings;
        }

        #endregion

        #region Methods

        public async Task Start(string host, UserAuthData authData)
        {
            await Start(host, authData, new ThermoDeviceManager(_settings), _settings.UpdateInterval, _settings.Timeout);
        }

        #endregion
    }
}
