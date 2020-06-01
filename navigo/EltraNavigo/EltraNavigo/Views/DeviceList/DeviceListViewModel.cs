using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EltraConnector.UserAgent;
using EltraCloudContracts.Contracts.Users;
using EltraNavigo.Controls;
using EltraNavigo.Views.Login;
using Xamarin.Forms;
using EltraCommon.Logger;
using EltraNavigo.Views.DeviceList.Events;
using EltraNavigo.Device.Vcs.Factory;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;

namespace EltraNavigo.Views.DeviceList
{
    public class DeviceListViewModel : ToolViewModel
    {
        #region Private fields

        private List<(Session, EltraDevice)> _sessionDevices;
        private List<DeviceViewModel> _devices;
        private DeviceViewModel _selectedDevice;
        private bool _isRefreshing;

        #endregion

        #region Constructors

        public DeviceListViewModel(DeviceAgent agent)
        {
            Title = "Devices";
            Image = ImageSource.FromResource("EltraNavigo.Resources.download_32px.png");
            Uuid = Application.Current.Properties["uuid"] as string;
            IsMandatory = true;
            Agent = agent;
            
            RegisterEvents();
        }

        #endregion

        #region Events

        public event EventHandler<SelectedDeviceEventArgs> SelectedDeviceChanged;

        protected virtual void OnSelectedDeviceChanged(SelectedDeviceEventArgs e)
        {
            if (Agent != null && e.Device != null)
            {
                Vcs.Device = e.Device.Device;
            }

            SelectedDeviceChanged?.Invoke(this, e);
        }

        #endregion

        #region Commands 

        public ICommand RefreshCommand => new Command(OnRefreshDevicesAsync);

        #endregion

        #region Properties

        public LoginViewModel LoginViewModel { get; set; }

        public List<DeviceViewModel> Devices
        {
            get => _devices ?? (_devices = new List<DeviceViewModel>());
            set => SetProperty(ref _devices, value);
        }

        public string Url
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("url"))
                {
                    result = Application.Current.Properties["url"] as string;
                }

                return result;
            }
        }

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if(SetProperty(ref _selectedDevice, value))
                {
                    OnSelectedDeviceChanged(new SelectedDeviceEventArgs(){ Device = SelectedDevice });
                }
            } 
        }
        
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public string Login
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("login"))
                {
                    result = Application.Current.Properties["login"] as string;
                }

                return result;
            }
        }

        public string Password
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("password"))
                {
                    result = Application.Current.Properties["password"] as string;
                }

                return result;
            }
        }

        public string Name
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("name"))
                {
                    result = Application.Current.Properties["name"] as string;
                }

                return result;
            }
        }

        public string DeviceLogin
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("user_login"))
                {
                    result = Application.Current.Properties["user_login"] as string;
                }

                return result;
            }
        }

        public string DevicePassword
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("user_password"))
                {
                    result = Application.Current.Properties["user_password"] as string;
                }

                return result;
            }
        }

        #endregion

        #region Events handler

        private async void OnDeviceListPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "SelectedDevice")
            {
                if(SelectedDevice!=null)
                {
                    await SelectedDevice.Show();
                }
            }
        }

        private void OnDeviceStatusChanged(object sender, EventArgs e)
        {
            if (sender is EltraDevice device)
            {
                if (device.Status == DeviceStatus.Ready)
                {
                    var session = FindDeviceSession(device);

                    if (!DeviceExists(device))
                    { 
                        AddDevice(session, device);
                    }
                }
            }
        }

        #endregion

        #region Methods

        private Session FindDeviceSession(EltraDevice device)
        {
            Session result = null;

            foreach(var sessionDevice in _sessionDevices)
            {
                var session = sessionDevice.Item1;
                
                if(session.Uuid == device.SessionUuid)
                {
                    result = session;
                    break;
                }                
            }

            return result;
        }

        private void RegisterEvents()
        {
            PropertyChanged += OnDeviceListPropertyChanged;
        }

        private void RegisterDeviceEvents(EltraCloudContracts.Contracts.Devices.EltraDevice device)
        {
            if(device!=null)
            { 
                device.StatusChanged -= OnDeviceStatusChanged;
                device.StatusChanged += OnDeviceStatusChanged;
            }
        }

        private async Task ClearDevices()
        {
            foreach (var device in Devices)
            {
                await device.Hide();
            }

            Devices = new List<DeviceViewModel>();
        }
        
        private async Task GetDevices()
        {
            try
            {
                if (Agent != null)
                {
                    _sessionDevices = await Agent.GetDevices(new UserAuthData { Login = DeviceLogin, Password = DevicePassword });

                    await ClearDevices();

                    if (_sessionDevices != null)
                    {
                        foreach (var sessionDevice in _sessionDevices)
                        {
                            var session = sessionDevice.Item1;
                            var device = sessionDevice.Item2;

                            RegisterDeviceEvents(device);

                            if (device.Status == DeviceStatus.Ready)
                            {
                                if (!DeviceExists(device))
                                {
                                    AddDevice(session, device);
                                }
                            }
                        }

                        SelectedDevice = Devices.FirstOrDefault();
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetDevices", e);
            }
        }

        private bool DeviceExists(EltraDevice newDevice)
        {
            bool result = false;

            if(newDevice != null)
            { 
                foreach(var viewModel in Devices)
                {
                    var existingDevice = viewModel.Device;
                    
                    if( existingDevice?.Version != null && newDevice.Version != null &&
                        existingDevice?.Version == newDevice.Version)
                    {
                        if ( existingDevice?.Identification?.SerialNumber !=null && newDevice.Identification?.SerialNumber != null &&
                             existingDevice?.Identification?.SerialNumber == newDevice.Identification?.SerialNumber)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }
        
        private void AddDevice(Session session, EltraDevice device)
        {
            var vcs = DeviceVcsFactory.CreateVcs(Agent, device);

            if (vcs != null)
            {
                Init(vcs);

                var deviceViewModel = new DeviceViewModel(this, session, device);
                var devices = new List<DeviceViewModel>(Devices);

                devices.Add(deviceViewModel);

                Devices = devices;
            }
        }

        private void OnRefreshDevicesAsync()
        {
            RefreshDevicesAsync();
        }

        public void RefreshDevicesAsync()
        {
            if (!IsRefreshing)
            {
                Task.Run(async () =>
                {
                    IsRefreshing = true;
                    IsBusy = true;

                    await GetDevices();

                    IsRefreshing = false;
                    IsBusy = false;
                }).ConfigureAwait(true);
            }
        }

        #endregion
    }
}