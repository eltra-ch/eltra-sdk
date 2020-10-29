using EltraCommon.Logger;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraCommon.Contracts.Parameters;
using EltraNavigoRelay.Views.RelayControl.ObjectDictionary;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using Xamarin.Essentials;
using EltraNavigoRelay.Device.Vcs;
using EltraNavigoRelay.Views.Relay.Base;
using System.Reflection;

namespace EltraNavigoRelay.Views.Relay.RelayControl
{
    public class RelayControlViewModel : RelayToolViewModel
    {
        #region Private fields

        private string[] _controlButtonText;
        private ushort[] _channelState;
        
        private bool[] _isControlButtonEnabled;
                
        private CancellationTokenSource[] _autoSwitchOffCancelationTokenSource;

        private Task[] _autoSwitchOffTask;
                
        private string[] _remainingTime;
        
        private bool[] _isSwitchedOn;
        
        private bool[] _isAutoSwitchOffActive;
        
        private Parameter[] _parameters;

        private int _channelCount = 2;

        SensorSpeed _speed = SensorSpeed.UI;

        #endregion

        #region Constructors

        public RelayControlViewModel()
        {
            Title = "Relay Control";
            Uuid = "53B3F06F-D044-48A7-9D42-4AA8D0B9F471";
        }

        #endregion

        #region Commands 

        public ICommand ControlButton1Command => new Command(OnControlButton1Pressed);
        public ICommand ControlButton2Command => new Command(OnControlButton2Pressed);

        #endregion

        #region Properties
       
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

        public string RemainingTime1
        { 
            get => _remainingTime[0];
            set => SetProperty(ref _remainingTime[0], value);
        }

        public string RemainingTime2
        {
            get => _remainingTime[1];
            set => SetProperty(ref _remainingTime[1], value);
        }

        public bool IsButton1SwitchedOn
        {
            get => _isSwitchedOn[0];
            set => SetProperty(ref _isSwitchedOn[0], value);
        }

        public bool IsButton2SwitchedOn
        {
            get => _isSwitchedOn[1];
            set => SetProperty(ref _isSwitchedOn[1], value);
        }

        public bool IsButton1AutoSwitchOffActive
        {
            get => _isAutoSwitchOffActive[0];
            set => SetProperty(ref _isAutoSwitchOffActive[0], value);
        }

        public bool IsButton2AutoSwitchOffActive
        {
            get => _isAutoSwitchOffActive[1];
            set => SetProperty(ref _isAutoSwitchOffActive[1], value);
        }

        #endregion

        #region Events handling

        public override void SetUp()
        {
            if (CanSetUp)
            {
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                var assemblyName = assembly.GetName();

                UpdateViewModels = false;

                _controlButtonText = new string[_channelCount];
                _channelState = new ushort[_channelCount];
                _autoSwitchOffTask = new Task[_channelCount];
                _autoSwitchOffCancelationTokenSource = new CancellationTokenSource[_channelCount];
                _isSwitchedOn = new bool[_channelCount];
                _isAutoSwitchOffActive = new bool[_channelCount];
                _isControlButtonEnabled = new bool[_channelCount];
                _remainingTime = new string[_channelCount];
                _parameters = new Parameter[_channelCount];

                Image = ImageSource.FromResource($"{assemblyName.Name}.Resources.hazardous_32px.png");

                for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
                {
                    _autoSwitchOffCancelationTokenSource[channelIndex] = new CancellationTokenSource();
                    _channelState[channelIndex] = 255;
                }

                PropertyChanged += OnViewModelPropertyChanged;
                DeviceInitialized += OnDeviceInitialized;

                ReadAutoSwitchOffSettings();
            }

            base.SetUp();
        }

        private void OnShakeDetected(object sender, EventArgs e)
        {
            ToggleSwitch1OnOff();
        }

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            const ushort relay1Index = 0x3141;

            UpdateRelayChannelState(e.Parameter.Index - relay1Index, e.Parameter.ActualValue);
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
            {
                if (e.PropertyName == $"IsButton{channelIndex}AutoSwitchOffActive")
                { 
                    Application.Current.Properties[$"auto_switch_{channelIndex+1}_off_active"] = _isAutoSwitchOffActive[channelIndex].ToString();
                }
            }                
        }

        private void OnDeviceInitialized(object sender, EventArgs e)
        {
            Device.ObjectDictionary = new RelayObjectDictionary(Device);
        }

        #endregion

        #region Methods

        private void ToggleSwitch1OnOff()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                await OnButtonPressed(0);
            });
        }

        public void EnableAccelerometer(bool enable)
        {
            try
            {
                if(enable)
                {
                    if (Accelerometer.IsMonitoring)
                    {
                        Accelerometer.Stop();
                    }

                    Accelerometer.ShakeDetected += OnShakeDetected;

                    Accelerometer.Start(_speed);                    
                }                
                else
                {
                    if (Accelerometer.IsMonitoring)
                    {
                        Accelerometer.ShakeDetected -= OnShakeDetected;

                        Accelerometer.Stop();                        
                    }
                }
            }
            catch (FeatureNotSupportedException)
            {
                MsgLogger.WriteError($"{GetType().Name} - EnableAccelerometer", "Accelerometer not supported on device");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - EnableAccelerometer", e);
            }
        }
               

        private void ReadAutoSwitchOffSettings()
        {
            for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
            { 
                if (Application.Current.Properties.ContainsKey($"auto_switch_{channelIndex+1}_off_active"))
                {
                    if (bool.TryParse(Application.Current.Properties[$"auto_switch_{channelIndex+1}_off_active"] as string, out bool autoSwitchOff))
                    {
                        _isAutoSwitchOffActive[channelIndex] = autoSwitchOff;
                    }               
                }

                OnPropertyChanged($"IsButton{channelIndex+1}AutoSwitchOffActive");
            }
        }

        private async void OnControlButton1Pressed(object obj)
        {
            await OnButtonPressed(0);
        }

        private async void OnControlButton2Pressed(object obj)
        {
            await OnButtonPressed(1);
        }

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
            if (RelayVcs != null)
            {
                try
                {
                    switch (_channelState[channelIndex])
                    {
                        case 1:
                            IsBusy = true;

                            EnableControlButton(channelIndex, false);

                            SetControlButtonText(channelIndex, "OFF");

                            var call = await RelayVcs.SetRelayChannelState((ushort)channelIndex, 0);

                            if(call == null || !call.Result)
                            {
                                SetControlButtonText(channelIndex, "ON");
                            }

                            IsBusy = false;

                            break;
                        case 0:
                            IsBusy = true;

                            if (_isAutoSwitchOffActive[channelIndex])
                            {
                                EnableControlButton(channelIndex, false);

                                _autoSwitchOffCancelationTokenSource[channelIndex].Cancel();
                                _autoSwitchOffTask[channelIndex]?.Wait();
                            }
                            else
                            {
                                SetControlButtonText(channelIndex, "ON");
                                EnableControlButton(channelIndex, false);

                                call = await RelayVcs.SetRelayChannelState((ushort)channelIndex, 1);

                                if (call == null || !call.Result)
                                {                                    
                                    SetControlButtonText(channelIndex, "OFF");
                                }
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

        private void SetRemainingTime(int channelIndex, TimeSpan remainingTime)
        {
            string timeFormat = @"mm\.ss";

            _remainingTime[channelIndex] = remainingTime.ToString(timeFormat);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"RemainingTime{channelIndex + 1}");
            });
        }

        private void SetSwitchedOn(int channelIndex, bool on)
        {
            _isSwitchedOn[channelIndex] = on;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"IsButton{channelIndex + 1}SwitchedOn");
            });            
        }

        private void AutoSwitchOff(int channelIndex, CancellationToken token)
        {
            const int minTimeout = 100;
            var watch = new Stopwatch();
            var maxRemainingTime = TimeSpan.FromMinutes(10);

            SetSwitchedOn(channelIndex, true);

            SetRemainingTime(channelIndex, maxRemainingTime);

            do
            {
                watch.Start();

                Thread.Sleep(minTimeout);

                SetRemainingTime(channelIndex, (maxRemainingTime - watch.Elapsed));

            } while (!token.IsCancellationRequested && watch.Elapsed < maxRemainingTime);
            
            SetRemainingTime(channelIndex, new TimeSpan());

            Task.Run(() => RelayVcs.SetRelayChannelState((ushort)channelIndex, 1)).ContinueWith(delegate {
                
                SetSwitchedOn(channelIndex, false);
                
                IsBusy = false;
            });            
        }

        private async Task UpdateRelayChannelState(int channelIndex)
        {
            if (Vcs != null)
            {
                var relayStateParameter = await Device.GetParameter($"PARAM_RelayState_{channelIndex + 1}");

                if (relayStateParameter != null)
                {
                    var relayState = await relayStateParameter.ReadValue();

                    if (relayState != null)
                    {
                        UpdateRelayChannelState(channelIndex, relayState);
                    }
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

                            if (_isAutoSwitchOffActive[channelIndex])
                            {
                                StartAutoSwitchOffTask(channelIndex);
                            }
                            else
                            {                                
                                SetSwitchedOn(channelIndex, true);
                            }
                        }
                        break;
                        case 1:
                        {
                            SetControlButtonText(channelIndex, "ON");

                            if (!_isAutoSwitchOffActive[channelIndex])
                            {
                                SetSwitchedOn(channelIndex, false);
                            }

                            EnableControlButton(channelIndex, true);
                        }
                        break;
                        case 255: 
                        {
                            EnableControlButton(channelIndex, false);
                        } break;
                    }

                    _channelState[channelIndex] = channelState;
                }
            }
        }

        private void StartAutoSwitchOffTask(int channelIndex)
        {
            _autoSwitchOffCancelationTokenSource[channelIndex] = new CancellationTokenSource();

            _autoSwitchOffTask[channelIndex] = Task.Run(() => AutoSwitchOff(channelIndex, _autoSwitchOffCancelationTokenSource[channelIndex].Token));
        }

        public override async Task Show()
        {
            IsBusy = true;

            EnableAccelerometer(true);

            ReadAutoSwitchOffSettings();

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
                    _parameters[channelIndex] = await Device.GetParameter($"PARAM_RelayState_{channelIndex + 1}");

                    if (_parameters[channelIndex] != null)
                    {
                        _parameters[channelIndex].ParameterChanged += OnParameterChanged;

                        _parameters[channelIndex].AutoUpdate(ParameterUpdatePriority.Medium);
                    }

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
                    if (_parameters[channelIndex] != null)
                    {
                        _parameters[channelIndex].StopUpdate(ParameterUpdatePriority.Medium);

                        _parameters[channelIndex].ParameterChanged -= OnParameterChanged;
                    }
                }
            }
        }

        public override async Task Hide()
        {
            EnableAccelerometer(false);

            UnregisterParameterUpdate();

            for (int channelIndex = 0; channelIndex < _channelCount; channelIndex++)
            {
                _autoSwitchOffCancelationTokenSource[channelIndex].Cancel();
                _autoSwitchOffTask[channelIndex]?.Wait();
            }
            
            await base.Hide();
        }

        #endregion
    }
}
