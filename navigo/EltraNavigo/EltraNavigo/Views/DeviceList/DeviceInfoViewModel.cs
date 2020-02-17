using EltraCloudContracts.Contracts.Sessions;
using EltraConnector.UserAgent;
using EltraConnector.UserAgent.Vcs;
using EltraNavigo.Controls;
using EltraNavigo.Views.DeviceList.Events;
using EltraNavigo.Views.DeviceList.Maps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigo.Views.DeviceList
{
    public class DeviceInfoViewModel : BaseViewModel
    {
        #region Private fields

        private DeviceVcs _vcs;        
        private string _login;
        private string _loginName;
        private string _location;
        private string _serialNumber;
        private List<Location> _locations;

        #endregion

        #region Constructors

        public DeviceInfoViewModel(DeviceViewModel parent, DeviceVcs vcs, Session session)
            : base(parent)
        {
            _vcs = vcs;

            UpdateSessionInfoAsync(session);
        }

        #endregion

        #region Commands 

        public ICommand DismissCommand => new Command(OnDismissCommand);

        #endregion

        #region Events

        public event EventHandler<LocationChangedEventArgs> LocationChanged;

        #endregion

        #region Event handling

        protected virtual void OnLocationChanged(LocationChangedEventArgs e)
        {
            LocationChanged?.Invoke(this, e);
        }

        private void OnDismissCommand(object obj)
        {
            var pageNavigation = Application.Current.MainPage.Navigation;

            pageNavigation.PopModalAsync(false);
        }

        #endregion

        #region Properties

        private DeviceViewModel DeviceViewModel => Parent as DeviceViewModel;

        private EltraCloudContracts.Contracts.Devices.EltraDevice Device => DeviceViewModel?.Device;

        private DeviceAgent Agent => _vcs?.Agent;

        public string Name
        {
            get => DeviceViewModel?.Name;
        }

        public string Description
        {
            get => DeviceViewModel?.Description;
        }

        public string Version
        {
            get => DeviceViewModel?.Version;
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string LoginName
        {
            get => _loginName;
            set => SetProperty(ref _loginName, value);
        }
        
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public List<Location> Locations
        {
            get => _locations ?? (_locations = new List<Location>());
            set => SetProperty(ref _locations, value);
        }

        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        #endregion

        #region Methods

        private void UpdateSessionInfoAsync(Session session)
        {
            Task.Run(() => {

                SerialNumber = $"0x{Device.Identification.SerialNumber:X}";

                Login = session?.User?.AuthData?.Login;
                LoginName = session?.User?.AuthData?.Name;
                
                if(session.IpLocation.Latitude == 0 && session.IpLocation.Longitude == 0)
                {
                    session.IpLocation.Latitude = 46.85992691;
                    session.IpLocation.Longitude = 8.22152048;
                }

                if(string.IsNullOrEmpty(session?.IpLocation.City))
                {
                    Location = "Sachseln, Switzerland";                    
                }
                else
                {
                    Location = $"{session?.IpLocation.City}, {session.IpLocation.Country}";
                }

                var location = new Location(session.IpLocation);

                Locations = new List<Location>() { location };

                OnLocationChanged(new LocationChangedEventArgs() { Position = location.Position });
            });
                        
        }

        #endregion
    }
}
