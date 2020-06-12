using System;
using System.Collections.Generic;
using EltraConnector.UserAgent;
using EltraNavigo.Controls;
using EltraNavigo.Views.About;
using EltraNavigo.Views.DataRecorder;
using EltraNavigo.Views.DeviceList;
using EltraNavigo.Views.Homing;
using EltraNavigo.Views.Login;
using EltraNavigo.Views.Obd;
using EltraNavigo.Views.Ppm;
using EltraNavigo.Views.Pvm;
using EltraCloudContracts.Contracts.ToolSet;
using EltraNavigo.Views.RelayControl;
using EltraNavigo.Views.DeviceList.Events;
using System.Threading.Tasks;
using EltraNavigo.Views.Devices.Thermo.Overview;
using EltraNavigo.Views.Devices.Thermo.Control;
using EltraNavigo.Views.Devices.Thermo.History;
using EltraNavigo.Views.Devices.Thermo.Settings;
using EltraNavigo.Views.PhotoControl;

namespace EltraNavigo.Views
{
    public class MasterViewModel : BaseViewModel
    {
        #region Private fields
        
        private DeviceAgent _agent;

        private List<ToolViewModel> _viewModels;
        private List<ToolViewModel> _headerViewModels;
        private List<ToolViewModel> _toolViewModels;
        private List<ToolViewModel> _footerViewModels;
        private List<ToolViewModel> _supportedViewModels;

        private DeviceListViewModel _deviceListViewModel;
        private DataRecorderViewModel _dataRecorderViewModel;
        
        private PvmViewModel _pvmViewModel;
        private PpmViewModel _ppmViewModel;
        private ObdViewModel _obdViewModel;
        private HomingViewModel _homingViewModel;
        
        private RelayControlViewModel _relayControlViewModel;
        
        private ThermoControlViewModel _thermoControlViewModel;
        private ThermoOverviewViewModel _thermoOverviewViewModel;
        private ThermoHistoryViewModel _thermoHistoryViewModel;
        private ThermoSettingsViewModel _thermoSettingsViewModel;

        private PhotoControlViewModel _photoControlViewModel;

        private LoginViewModel _loginViewModel;
        private AboutViewModel _aboutViewModel;

        private ToolViewModel _activeViewModel;
        private ToolViewModel _previousViewModel;

        private bool _deviceLockedOnStop;
        private bool _useLastPage = true;

        #endregion

        #region Constructors

        public MasterViewModel(BaseViewModel parent)
            : base(parent)
        {
            AddModels();
        }

        #endregion

        #region Events

        public event EventHandler PageChanged;

        #endregion

        #region Event handling

        protected virtual void OnPageChanged()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public string LastUsedPageName
        {
            get
            {
                string result = string.Empty;

                if(App.Current.Properties.ContainsKey("last_used_page"))
                { 
                    result = App.Current.Properties["last_used_page"] as string;
                }

                if(string.IsNullOrEmpty(result))
                {
                    result = DeviceListViewModel.Uuid;
                }

                return result;
            }
            set
            {
                App.Current.Properties["last_used_page"] = value;
            }             
        }

        public ToolViewModel LastUsedPage
        {
            get
            {
                ToolViewModel result = DeviceListViewModel;

                foreach (var page in _viewModels)
                {
                    if( page.Uuid == LastUsedPageName)
                    {
                        result = page;
                        break;
                    }
                }

                return result;
            }
        }

        public DeviceAgent Agent
        {
            get => _agent;
            set
            {
                _agent = value;
                OnAgentChanged();
            }
        }

        public List<ToolViewModel> ViewModels
        {
            get => _viewModels ?? (_viewModels = new List<ToolViewModel>());
            set => SetProperty(ref _viewModels, value);
        }

        public List<ToolViewModel> SupportedViewModels
        {
            get => _supportedViewModels ?? (_supportedViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _supportedViewModels, value);
        }

        public List<ToolViewModel> HeaderViewModels
        {
            get => _headerViewModels ?? (_headerViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _headerViewModels, value);
        }

        public List<ToolViewModel> ToolViewModels
        {
            get => _toolViewModels ?? (_toolViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _toolViewModels, value);
        }

        public List<ToolViewModel> FooterViewModels
        {
            get => _footerViewModels ?? (_footerViewModels = new List<ToolViewModel>());
            set => SetProperty(ref _footerViewModels, value);
        }

        public ToolViewModel ActiveViewModel
        {
            get => _activeViewModel;
            set => SetProperty(ref _activeViewModel, value);
        }

        public DeviceListViewModel DeviceListViewModel => _deviceListViewModel ?? (_deviceListViewModel = CreateDeviceListViewModel());

        public DataRecorderViewModel DataRecorderViewModel => _dataRecorderViewModel ?? (_dataRecorderViewModel = new DataRecorderViewModel());

        public PvmViewModel PvmViewModel => _pvmViewModel ?? (_pvmViewModel = new PvmViewModel());

        public PpmViewModel PpmViewModel => _ppmViewModel ?? (_ppmViewModel = new PpmViewModel());

        public HomingViewModel HomingViewModel => _homingViewModel ?? (_homingViewModel = new HomingViewModel());

        public ObdViewModel ObdViewModel => _obdViewModel ?? (_obdViewModel = new ObdViewModel());

        public RelayControlViewModel RelayControlViewModel => _relayControlViewModel ?? (_relayControlViewModel = new RelayControlViewModel());

        public PhotoControlViewModel PhotoControlViewModel => _photoControlViewModel ?? (_photoControlViewModel = new PhotoControlViewModel());

        public ThermoControlViewModel ThermoControlViewModel => _thermoControlViewModel ?? (_thermoControlViewModel = new ThermoControlViewModel());

        public ThermoOverviewViewModel ThermoOverviewViewModel => _thermoOverviewViewModel ?? (_thermoOverviewViewModel = new ThermoOverviewViewModel());

        public ThermoHistoryViewModel ThermoHistoryViewModel => _thermoHistoryViewModel ?? (_thermoHistoryViewModel = new ThermoHistoryViewModel());

        public ThermoSettingsViewModel ThermoSettingsViewModel => _thermoSettingsViewModel ?? (_thermoSettingsViewModel = new ThermoSettingsViewModel());

        public void ResetAgent()
        {
            _agent = null;

            foreach (var viewModel in _viewModels)
            {
                viewModel.ResetAgent();
            }
        }

        public LoginViewModel LoginViewModel => _loginViewModel ?? (_loginViewModel = CreateLoginViewModel());

        public AboutViewModel AboutViewModel => _aboutViewModel ?? (_aboutViewModel = new AboutViewModel());

        #endregion

        #region Events handling
        
        private void OnAgentChanged()
        {
            foreach(var viewModel in _viewModels)
            {
                viewModel.Agent = Agent;
            }
        }

        private async void OnSelectedDeviceChanged(object sender, SelectedDeviceEventArgs e)
        {            
            var deviceViewModel = e.Device;

            if (deviceViewModel != null)
            {
                Agent?.Clear();

                var toolSet = deviceViewModel.Device?.ToolSet;

                if(toolSet!=null)
                {
                    var supportedViewModels = new List<ToolViewModel>();

                    supportedViewModels.AddRange(HeaderViewModels);

                    foreach (var viewModel in ViewModels)
                    {
                        var tool = toolSet.FindToolByUuid(viewModel.Uuid);

                        if((tool!=null && tool.Status == DeviceToolStatus.Enabled) || viewModel.IsMandatory)
                        {
                            viewModel.IsSupported = true;

                            viewModel.Device = deviceViewModel.Device;

                            if(!ViewModelExists(supportedViewModels, viewModel))
                            { 
                                supportedViewModels.Add(viewModel);
                            }
                        }
                        else if((tool != null && tool.Status == DeviceToolStatus.Disabled) || !viewModel.IsMandatory)
                        {
                            viewModel.IsSupported = false;
                                
                            if(viewModel.IsVisible)
                            {
                                await viewModel.Hide();
                            }

                            if(ActiveViewModel == viewModel)
                            {
                                GotoLastUsedPage(); 
                            }
                        }                            
                    }

                    supportedViewModels.AddRange(FooterViewModels);

                    SupportedViewModels = supportedViewModels;

                    if(_useLastPage)
                    {
                        GotoLastUsedPage();

                        _useLastPage = false;
                    }
                }                    
            }
        }

        #endregion

        #region Methods

        public void GotoFirstPage()
        {
            if (!LoginViewModel.AutoLogOnActive)
            {
                ChangePage(LoginViewModel, true);
            }
            else
            {
                ChangePage(DeviceListViewModel, true);

                DeviceListViewModel.RefreshDevicesAsync();
            }
        }

        private void GotoLastUsedPage()
        {
            if(LastUsedPage.IsSupported)
            {
                _deviceLockedOnStop = true;

                LockOnStart();

                ChangePage(LastUsedPage, true);
            }
        }

        public async void ChangePage(ToolViewModel viewModel, bool internalChange = false)
        {
            if (viewModel != null && _activeViewModel != viewModel)
            {
                _previousViewModel = _activeViewModel;

                if (_previousViewModel != null)
                {
                    try
                    {
                        await _previousViewModel.Hide();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                try
                {
                    await viewModel.Show();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                ActiveViewModel = viewModel;

                if(!internalChange)
                { 
                    LastUsedPageName = viewModel.Uuid;
                }

                Title = ActiveViewModel.Title;

                OnPageChanged();
            }
        }

        private bool ViewModelExists(List<ToolViewModel> viewModels, ToolViewModel toolViewModel)
        {
            bool result = false;

            if (viewModels != null && toolViewModel != null)
            {
                foreach (var viewModel in viewModels)
                {
                    if (viewModel.Uuid == toolViewModel.Uuid)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private LoginViewModel CreateLoginViewModel()
        {
            var  result = new UserSignInViewModel();

            result.Changed += (sender, args) =>
            {
                ChangePage(DeviceListViewModel, true);

                DeviceListViewModel.RefreshDevicesAsync();
            };

            result.Canceled += (sender, args) =>
            {
                ChangePage(_previousViewModel ?? DeviceListViewModel, true);
            };

            return result;
        }

        private DeviceListViewModel CreateDeviceListViewModel()
        {
            var result = new DeviceListViewModel(Agent) { LoginViewModel = LoginViewModel };

            RegisterEvents(result);

            return result;
        }

        private void RegisterEvents(DeviceListViewModel deviceListViewModel)
        {
            deviceListViewModel.SelectedDeviceChanged += OnSelectedDeviceChanged;
        }

        private void AddModels()
        {
            ToolViewModels = new List<ToolViewModel>
            {
                ObdViewModel,
                PvmViewModel,
                PpmViewModel,
                HomingViewModel,
                DataRecorderViewModel,
                RelayControlViewModel,
                PhotoControlViewModel,
                ThermoOverviewViewModel,
                ThermoControlViewModel,
                ThermoHistoryViewModel,
                ThermoSettingsViewModel
            };

            HeaderViewModels = new List<ToolViewModel>
            {
                LoginViewModel,
                DeviceListViewModel                
            };

            FooterViewModels = new List<ToolViewModel>
            {
                AboutViewModel
            };

            var viewModels = new List<ToolViewModel>();
            
            viewModels.AddRange(HeaderViewModels);
            viewModels.AddRange(ToolViewModels);
            viewModels.AddRange(FooterViewModels);

            ViewModels = viewModels;

            var supportedViewModels = new List<ToolViewModel>();

            supportedViewModels.AddRange(HeaderViewModels);
            supportedViewModels.AddRange(FooterViewModels);

            SupportedViewModels = supportedViewModels;

            GotoFirstPage();
        }

        public void LockOnStart()
        {
            var deviceViewModel = DeviceListViewModel.SelectedDevice;

            if (deviceViewModel != null)
            {
                if (_deviceLockedOnStop)
                {
                    if (deviceViewModel.CanLockDevice)
                    {
                        deviceViewModel.LockToggle(true);
                    }
                }

                _deviceLockedOnStop = false;
            }
        }

        public void UnlockOnStop()
        {
            var deviceViewModel = DeviceListViewModel.SelectedDevice;

            if (deviceViewModel != null)
            {
                _deviceLockedOnStop = deviceViewModel.IsDeviceLocked;

                if (_deviceLockedOnStop)
                {
                    deviceViewModel.LockToggle(false);
                }
            }
        }

        public async Task StopUpdate()
        {
            foreach (var viewModel in SupportedViewModels)
            {
                await viewModel.StopUpdate();
            }    
        }

        public async Task StartUpdate()
        {
            foreach (var viewModel in SupportedViewModels)
            {
                await viewModel.StartUpdate();
            }
        }

        #endregion        
    }
}
