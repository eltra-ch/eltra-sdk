using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraXamCommon.Controls;
using System.Timers;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using EltraNavigoStreema.Views.Devices.Streema.StreemaControl.Station;
using System.Reflection;

namespace EltraNavigoStreema.Views.StreemaControl
{
    public class StreemaControlViewModel : XamToolViewModel
    {
        #region Private fields

        private double _volumeValue;
        private bool _isMuteActive;
        private bool _internalChange;
        private ushort _statusWordValue;

        private XddParameter _volumeParameter;
        private XddParameter _muteParameter;
        private XddParameter _statusWordParameter;
        
        private Timer _valumeHistereseTimer;

        private List<StreemaStationViewModel> _stationList;

        #endregion

        #region Constructors

        public StreemaControlViewModel()
        {
            Title = "Streema Control";
            Uuid = "A8BEC143-7BBE-4C3B-A425-6C2D38C32E1A";
        }

        #endregion

        #region Properties

        public List<StreemaStationViewModel> StationList
        {
            get => _stationList ?? (_stationList = new List<StreemaStationViewModel>());
            set => SetProperty(ref _stationList, value);
        }

        public ushort StatusWordValue
        {
            get => _statusWordValue;
            set => SetProperty(ref _statusWordValue, value);
        }

        public bool IsMuteActive
        {
            get => _isMuteActive;
            set => SetProperty(ref _isMuteActive, value);
        }

        public double VolumeValue
        {
            get => _volumeValue;
            set => SetProperty(ref _volumeValue, value);
        }

        #endregion

        #region Events handling

        private void OnDeviceInitialized(object sender, EventArgs e)
        {
            InitializeStateMachineParameter();

            InitializeVolumeParameter();

            InitializeMuteParameter();
            
            InitializeStationList();
        }

        private void InitializeStationList()
        {
            var stationList = new List<StreemaStationViewModel>();
            var stationsCountParameter = Device.SearchParameter("PARAM_StationsCount") as XddParameter;

            if (stationsCountParameter != null && stationsCountParameter.GetValue(out ushort maxCount))
            {
                for (int i = 0; i < maxCount; i++)
                {
                    var stationViewModel = new StreemaStationViewModel(this, i);

                    stationList.Add(stationViewModel);
                }
            }

            foreach (var station in stationList)
            {
                station.Agent = Agent;
                station.Device = Device;
            }

            StationList = stationList;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsMuteActive")
            {
                if (!_internalChange)
                {
                    Task.Run(async () =>
                    {
                        if (_muteParameter != null && _muteParameter.SetValue(IsMuteActive))
                        {
                            if (!await _muteParameter.Write())
                            {
                                _internalChange = true;

                                IsMuteActive = !IsMuteActive;

                                _internalChange = false;
                            }
                        }
                    });
                }
            }
        }

        private void InitializeMuteParameter()
        {
            _muteParameter = Device.SearchParameter(0x4201, 0x00) as XddParameter;

            if (_muteParameter != null)
            {
                _muteParameter.ParameterChanged += OnVolumeParameterChanged;

                Task.Run(async () =>
                {

                    await _muteParameter.UpdateValue();

                }).ContinueWith((t) =>
                {

                    if (_muteParameter.GetValue(out bool val))
                    {
                        IsMuteActive = val;
                    }
                });
            }
        }

        private void InitializeVolumeParameter()
        {
            _volumeParameter = Device.SearchParameter(0x4200, 0x00) as XddParameter;

            if (_volumeParameter != null)
            {
                _volumeParameter.ParameterChanged += OnVolumeParameterChanged;

                Task.Run(async () =>
                {

                    await _volumeParameter.UpdateValue();

                }).ContinueWith((t) =>
                {

                    if (_volumeParameter.GetValue(out int volumeValue))
                    {
                        VolumeValue = volumeValue;
                    }
                });
            }
        }

        private void InitializeStateMachineParameter()
        {
            _statusWordParameter = Device.SearchParameter("PARAM_StatusWord") as XddParameter;

            if (_statusWordParameter != null)
            {
                _statusWordParameter.ParameterChanged += OnStatusWordParameterChanged;

                Task.Run(async () =>
                {

                    await _statusWordParameter.UpdateValue();

                }).ContinueWith((t) =>
                {

                    if (_statusWordParameter.GetValue(out ushort val))
                    {
                        StatusWordValue = val;
                    }
                });
            }
        }
                
        private void OnVolumeParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if(e.Parameter is Parameter volumeParameter)
            {
                if (volumeParameter.GetValue(out int volumeValue))
                {
                    VolumeValue = volumeValue;
                }
            }
        }

        private void OnStatusWordParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter is Parameter statusWordParameter)
            {
                if (statusWordParameter.GetValue(out ushort val))
                {
                    StatusWordValue = val;
                }
            }
        }

        private void OnVolumeHistereseElapsed(object sender, ElapsedEventArgs e)
        {
            _valumeHistereseTimer.Stop();

            OnVolumeChanged();
        }

        #endregion

        #region Methods

        public override void SetUp()
        {
            if (CanSetUp)
            {
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                var assemblyName = assembly.GetName();

                Image = ImageSource.FromResource($"{assemblyName.Name}.Resources.music_32px.png");

                UpdateViewModels = false;

                DeviceInitialized += OnDeviceInitialized;

                PropertyChanged += OnViewModelPropertyChanged;
            }

            base.SetUp();
        }

        private void CreateVolumeHistereseTimer()
        {
            if(_valumeHistereseTimer!=null)
            {
                _valumeHistereseTimer.Stop();
                _valumeHistereseTimer.Dispose();
            }

            _valumeHistereseTimer = new Timer(500);
            _valumeHistereseTimer.Elapsed += OnVolumeHistereseElapsed;
            _valumeHistereseTimer.Enabled = true;
            _valumeHistereseTimer.AutoReset = true;
        }

        private void OnVolumeChanged()
        {
            if(_volumeParameter != null)
            {
                int volumeValue = Convert.ToInt32(VolumeValue);

                if(_volumeParameter.SetValue(volumeValue))
                {
                    Task.Run(async ()=> {

                        Debug.Print($"before write, new value = {volumeValue}\r\n");

                        if (await _volumeParameter.Write())
                        {
                            Debug.Print($"after write, new value = {volumeValue}\r\n");
                        }
                        else
                        {
                            Debug.Print($"after write failed, value = {volumeValue}\r\n");
                        }                        
                    });
                }
            }
        }

        public void SliderVolumeValueChanged(double newValue)
        {
            if(_volumeParameter != null && _volumeParameter.GetValue(out int volumeValue))
            {
                int newIntValue = Convert.ToInt32(newValue);

                if (volumeValue != newIntValue)
                {
                    _volumeValue = Math.Round(newValue);

                    CreateVolumeHistereseTimer();
                }
            }
        }

        #endregion
    }
}
