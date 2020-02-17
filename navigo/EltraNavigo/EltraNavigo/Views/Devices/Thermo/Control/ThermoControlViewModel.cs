using EltraCommon.Logger;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigo.Device.Vcs;
using EltraNavigo.Views.Devices.Thermo.Base;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Devices.Thermo.Control
{
    public class ThermoControlViewModel : ThermoToolViewModel
    {
        #region Private fields

        private string[] _controlButtonText;
        private ushort[] _channelState;

        private bool[] _isControlButtonEnabled;

        private Parameter[] _channelParameters;

        private int _channelCount = 2;

        private ParameterEditViewModel _temperatureMin;
        private ParameterEditViewModel _temperatureMax;

        private ParameterEditViewModel _humidityMin;
        private ParameterEditViewModel _humidityMax;

        private BasicSensorDataViewModel _sensorDataViewModel;

        #endregion

        #region Constructors

        public ThermoControlViewModel()
        {
            UpdateViewModels = false;

            _controlButtonText = new string[_channelCount];
            _channelState = new ushort[_channelCount];
            _isControlButtonEnabled = new bool[_channelCount];
            _channelParameters = new Parameter[_channelCount];

            Title = "Control";
            Image = ImageSource.FromResource("EltraNavigo.Resources.hazardous_32px.png");
            Uuid = "6973CFB4-B798-4204-9C66-569CD0CB6BDA";

            for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
            {
                _channelState[channelIndex] = 255;
            }

            _temperatureMin = new ParameterEditViewModel(this, "PARAM_TemperatureMinThreshold");
            _temperatureMax = new ParameterEditViewModel(this, "PARAM_TemperatureMaxThreshold");

            _humidityMin = new ParameterEditViewModel(this, "PARAM_HumidityMinThreshold");
            _humidityMax = new ParameterEditViewModel(this, "PARAM_HumidityMaxThreshold");
        }

        #endregion

        #region Commands 

        public ICommand ControlButton1Command => new Command(OnControlButton1Pressed);
        public ICommand ControlButton2Command => new Command(OnControlButton2Pressed);

        #endregion

        #region Properties

        public BasicSensorDataViewModel SensorDataViewModel
        {
            get => _sensorDataViewModel ?? (_sensorDataViewModel = new BasicSensorDataViewModel(this));
            set => SetProperty(ref _sensorDataViewModel, value);
        }

        public ParameterEditViewModel TemperatureMin
        {
            get => _temperatureMin;
            set => SetProperty(ref _temperatureMin, value);
        }
        public ParameterEditViewModel TemperatureMax
        {
            get => _temperatureMax;
            set => SetProperty(ref _temperatureMax, value);
        }

        public ParameterEditViewModel HumidityMin
        {
            get => _humidityMin;
            set => SetProperty(ref _humidityMin, value);
        }

        public ParameterEditViewModel HumidityMax
        {
            get => _humidityMax;
            set => SetProperty(ref _humidityMax, value);
        }

        public string ControlButton1Text
        {
            get => _controlButtonText[0];
            set => SetProperty(ref _controlButtonText[0], value);
        }

        public string ControlButton2Text
        {
            get => _controlButtonText[1];
            set => SetProperty(ref _controlButtonText[1], value);
        }

        public bool IsControlButton1Enabled
        {
            get => _isControlButtonEnabled[0];
            set => SetProperty(ref _isControlButtonEnabled[0], value);
        }

        public bool IsControlButton2Enabled
        {
            get => _isControlButtonEnabled[1];
            set => SetProperty(ref _isControlButtonEnabled[1], value);
        }

        #endregion

        #region Events handling

        private void OnRelayParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            UpdateRelayChannelState(e.Parameter.SubIndex - 1, e.Parameter.ActualValue);
        }

        private async void OnControlButton1Pressed(object obj)
        {
            await OnButtonPressed(0);
        }

        private async void OnControlButton2Pressed(object obj)
        {
            await OnButtonPressed(1);
        }

        #endregion

        #region Methods

        private void SetControlButtonText(int channelIndex, string text)
        {
            _controlButtonText[channelIndex] = text;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"ControlButton{channelIndex + 1}Text");
            });
        }

        private void EnableControlButton(int channelIndex, bool enable)
        {
            _isControlButtonEnabled[channelIndex] = enable;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"IsControlButton{channelIndex + 1}Enabled");
            });
        }

        private async Task OnButtonPressed(int channelIndex)
        {
            if (ThermoVcs != null)
            {
                try
                {
                    switch (_channelState[channelIndex])
                    {
                        case 1:
                            IsBusy = true;

                            EnableControlButton(channelIndex, false);

                            SetControlButtonText(channelIndex, "OFF");

                            var call = await ThermoVcs.SetRelayChannelState((ushort)channelIndex, 0);

                            if (call == null || !call.Result)
                            {
                                SetControlButtonText(channelIndex, "ON");
                            }

                            IsBusy = false;

                            break;
                        case 0:
                            IsBusy = true;

                            SetControlButtonText(channelIndex, "ON");
                            EnableControlButton(channelIndex, false);

                            call = await ThermoVcs.SetRelayChannelState((ushort)channelIndex, 1);

                            if (call == null || !call.Result)
                            {
                                SetControlButtonText(channelIndex, "OFF");
                            }

                            IsBusy = false;

                            break;
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - OnButtonPressed", e);
                }
            }
        }

        private async Task UpdateRelayChannelState(int channelIndex)
        {
            if (Vcs != null)
            {
                var relayState = await Vcs.GetParameterValue($"PARAM_RelayState_{channelIndex + 1}");

                if (relayState != null)
                {
                    UpdateRelayChannelState(channelIndex, relayState);
                }
            }
        }

        private void UpdateRelayChannelState(int channelIndex, ParameterValue relayState)
        {
            ushort channelState = 255;

            if (relayState != null)
            {
                relayState.GetValue(ref channelState);

                if (_channelState[channelIndex] != channelState)
                {
                    switch (channelState)
                    {
                        case 0:
                            {
                                SetControlButtonText(channelIndex, "OFF");

                                EnableControlButton(channelIndex, true);
                            }
                            break;
                        case 1:
                            {
                                SetControlButtonText(channelIndex, "ON");

                                EnableControlButton(channelIndex, true);
                            }
                            break;
                        case 255:
                            {
                                EnableControlButton(channelIndex, false);
                            }
                            break;
                    }

                    _channelState[channelIndex] = channelState;
                }
            }
        }

        public override async Task Show()
        {
            IsBusy = true;

            TemperatureMin.InitModelData();
            TemperatureMax.InitModelData();

            HumidityMin.InitModelData();
            HumidityMax.InitModelData();

            await RegisterParameterUpdate();

            await base.Show();

            IsBusy = false;
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                await RegisterParameterUpdate();
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                UnregisterParameterUpdate();
            }

            return result;
        }

        private async Task RegisterParameterUpdate()
        {
            if (Vcs != null)
            {
                for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
                {
                    _channelParameters[channelIndex] = await Vcs.GetParameter($"PARAM_RelayState_{channelIndex + 1}");

                    if (_channelParameters[channelIndex] != null)
                    {
                        _channelParameters[channelIndex].ParameterChanged += OnRelayParameterChanged;
                    }

                    Vcs.RegisterParameterUpdate($"PARAM_RelayState_{channelIndex + 1}", ParameterUpdatePriority.Medium);

                    await UpdateRelayChannelState(channelIndex);
                }
            }
        }

        private void UnregisterParameterUpdate()
        {
            if (Vcs != null)
            {
                for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
                {
                    Vcs.UnregisterParameterUpdate($"PARAM_RelayState_{channelIndex + 1}", ParameterUpdatePriority.Medium);

                    if (_channelParameters[channelIndex] != null)
                    {
                        _channelParameters[channelIndex].ParameterChanged -= OnRelayParameterChanged;
                    }
                }
            }
        }

        public override async Task Hide()
        {
            UnregisterParameterUpdate();

            await base.Hide();
        }

        #endregion
    }
}
