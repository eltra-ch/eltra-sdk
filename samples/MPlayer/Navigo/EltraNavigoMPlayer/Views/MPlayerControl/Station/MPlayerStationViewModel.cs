using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraConnector.UserAgent.Definitions;
using EltraXamCommon.Controls;
using EltraXamCommon.Controls.Parameters;
using EltraXamCommon.Controls.Toast;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigoMPlayer.Views.MPlayerControl.Station
{
    public class MPlayerStationViewModel : ToolViewModel
    {
        #region Private fields

        private int _stationIndex;
        private int _previousStationIndex;
        private int _activeStationValue;
        private string _controlButtonText;
        private XddParameter _activeStationParameter;
        private string _stationStreamTitle;
        private bool _isStationEditVisible;
        private ParameterEditViewModel _stationIdParameter;
        private ParameterEditViewModel _stationVolumeScalingParameter;
        private bool _isActiveStation;
        private bool _deviceInitialization;

        #endregion

        #region Constructors

        public MPlayerStationViewModel(ToolViewBaseModel parent, int stationIndex)
            : base(parent)
        {
            _previousStationIndex = 0;
            _stationIndex = stationIndex;

            _stationIdParameter = new ParameterEditViewModel(this, $"PARAM_Station_{stationIndex+1}_Id");   
            _stationVolumeScalingParameter = new ParameterEditViewModel(this, $"PARAM_Station_{stationIndex+1}_VolumeScaling");

            DeviceInitialized += OnDeviceInitialized;
        }

        #endregion

        #region Commands 

        public ICommand ControlButtonCommand => new Command(OnControlButtonPressed);
        
        public ICommand EditButtonCommand => new Command(OnEditButtonPressed);

        #endregion

        #region Properties

        public bool IsActiveStation
        {
            get => _isActiveStation;
            set => SetProperty(ref _isActiveStation, value);
        }

        public int ActiveStationValue
        {
            get => _activeStationValue;
            set => SetProperty(ref _activeStationValue, value);
        }

        public string ControlButtonText
        {
            get => _controlButtonText;
            set => SetProperty(ref _controlButtonText, value);
        }

        public string StationStreamTitle
        {
            get => _stationStreamTitle;
            set => SetProperty(ref _stationStreamTitle, value);
        }

        public bool IsStationEditVisible
        {
            get => _isStationEditVisible;
            set => SetProperty(ref _isStationEditVisible, value);
        }

        public ParameterEditViewModel StationIdParameter
        {
            get => _stationIdParameter;
        }

        public ParameterEditViewModel StationVolumeScalingParameter
        {
            get => _stationVolumeScalingParameter;
        }

        #endregion

        #region Events handling

        private void OnStationIdChanged(object sender, ParameterChangedEventArgs e)
        {
            OnControlButtonPressed();
        }

        private void OnDeviceInitialized(object sender, EventArgs e)
        {
            _deviceInitialization = true;

            InitAgentStatus();

            InitializeActiveStationParameter();

            InitializeStationParameter();

            _deviceInitialization = false;
        }

        private void InitAgentStatus()
        {
            Agent.StatusChanged -= OnAgentStatusChanged;
            Agent.StatusChanged += OnAgentStatusChanged;

            IsEnabled = (Agent.Status == AgentStatus.Bound);
        }

        private void OnAgentStatusChanged(object sender, AgentStatusEventArgs e)
        {
            IsEnabled = (e.Status == AgentStatus.Bound);
        }

        private void OnEditButtonPressed(object obj)
        {
            IsStationEditVisible = !IsStationEditVisible;
        }

        private void OnActiveStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter is Parameter activeStationParameter)
            {
                if (activeStationParameter.GetValue(out int activeStationValue))
                {
                    ActiveStationValue = activeStationValue;

                    IsActiveStation = ActiveStationValue == (_stationIndex + 1);
                }
            }
        }

        private void OnStationLabelParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter.GetValue(out string label))
            {
                ControlButtonText = label;
            }
        }
        private void OnStreamTitleStationLabelParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter.GetValue(out string label))
            {
                StationStreamTitle = label;
            }
        }

        private void OnStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (!_deviceInitialization)
            {
                if (IsActiveStation)
                {
                    TurnOff();
                }
            }
        }

        private void OnControlButtonPressed(object obj = null)
        {
            if (!_deviceInitialization)
            {
                Task.Run(async () =>
                {
                    IsBusy = true;

                    await ActiveSelection(_stationIndex + 1);

                    IsBusy = false;
                });
            }
        }

        #endregion

        #region Methods

        public void TurnOff()
        {
            if (!_deviceInitialization)
            {
                Task.Run(async () =>
                {
                    IsBusy = true;

                    await ActiveSelection(0);

                    IsBusy = false;
                });
            }
        }

        public override Task Show()
        {
            var result = base.Show();

            StationIdParameter.InitModelData();
            StationIdParameter.Changed += OnStationIdChanged; 

            StationVolumeScalingParameter.InitModelData();
            
            return result;
        }

        public override Task Hide()
        {
            StationIdParameter.Changed -= OnStationIdChanged;

            return base.Hide();
        }

        private void InitializeStationParameter()
        {
            ushort index = (ushort)(0x4000 + (ushort)_stationIndex);
            var urlStationParameter = Device.SearchParameter(index, 0x01) as XddParameter;
            var labelStationParameter = Device.SearchParameter(index, 0x02) as XddParameter;
            var streamTitleStationParameter = Device.SearchParameter(index, 0x03) as XddParameter;

            if (urlStationParameter != null && labelStationParameter != null && streamTitleStationParameter != null)
            {
                string url;
                string label;
                string streamLabel;

                if (urlStationParameter.GetValue(out url))
                {
                    ControlButtonText = url;
                }

                if (labelStationParameter.GetValue(out label))
                {
                    ControlButtonText = label;
                }

                if (streamTitleStationParameter.GetValue(out streamLabel))
                {
                    StationStreamTitle = streamLabel;
                }

                labelStationParameter.ParameterChanged += OnStationLabelParameterChanged;
                streamTitleStationParameter.ParameterChanged += OnStreamTitleStationLabelParameterChanged;
                urlStationParameter.ParameterChanged += OnStationParameterChanged;

                labelStationParameter.AutoUpdate();
                streamTitleStationParameter.AutoUpdate();
                urlStationParameter.AutoUpdate();

                Task.Run(async () =>
                {
                    IsBusy = true;

                    await urlStationParameter.UpdateValue();
                    await labelStationParameter.UpdateValue();
                    await streamTitleStationParameter.UpdateValue();

                    IsBusy = false;
                }).ContinueWith((t) =>
                {
                    if (urlStationParameter.GetValue(out url))
                    {
                        ControlButtonText = url;
                    }

                    if (labelStationParameter.GetValue(out label))
                    {
                        ControlButtonText = label;
                    }

                    if (streamTitleStationParameter.GetValue(out streamLabel))
                    {
                        StationStreamTitle = streamLabel;
                    }
                });
            }
        }

        private void InitializeActiveStationParameter()
        {
            _activeStationParameter = Device.SearchParameter(0x4100, 0x00) as XddParameter;

            if (_activeStationParameter != null)
            {
                int activeStationValue = 0;

                if (_activeStationParameter.GetValue(out activeStationValue))
                {
                    ActiveStationValue = activeStationValue;

                    IsActiveStation = ActiveStationValue == (_stationIndex + 1);
                }

                _activeStationParameter.ParameterChanged += OnActiveStationParameterChanged;
                _activeStationParameter.AutoUpdate();

                Task.Run(async () =>
                {
                    IsBusy = true;

                    await _activeStationParameter.UpdateValue();

                    IsBusy = false;
                }).ContinueWith((t) =>
                {
                    if (_activeStationParameter.GetValue(out activeStationValue))
                    {
                        ActiveStationValue = activeStationValue;

                        IsActiveStation = ActiveStationValue == (_stationIndex + 1);
                    }
                });
            }
        }

        private async Task<bool> ActiveSelection(int index)
        {
            bool result = false;

            IsBusy = true;

            if (_previousStationIndex != index)
            {
                result = await SetActiveStation(index);

                _previousStationIndex = index;
            }
            else
            {
                if (_previousStationIndex != 0 && index > 0)
                {
                    if (await SetActiveStation(0))
                    {
                        result = await SetActiveStation(index);
                    }
                }
            }

            IsBusy = false;

            return result;
        }

        private async Task<bool> SetActiveStation(int index)
        {
            bool result = false;

            if (_activeStationParameter != null && _activeStationParameter.SetValue(index))
            {
                IsBusy = true;

                if (!await _activeStationParameter.Write())
                {
                    ToastMessage.ShortAlert($"Activate Button {index + 1} failed!");
                }
                else
                {
                    result = true;
                }

                IsBusy = false;
            }

            return result;
        }

        #endregion
    }
}
