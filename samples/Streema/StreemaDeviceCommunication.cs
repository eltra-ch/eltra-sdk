using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraConnector.Master.Device;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using EltraCommon.Logger;
using System.Runtime.InteropServices;

namespace StreemaMaster
{
    public class StreemaDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly List<Parameter> _urlParameters;

        private Parameter _activeStationParameter;
        private Parameter _volumeParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        private StreemaSettings _settings;

        #endregion

        #region Constructors

        public StreemaDeviceCommunication(MasterDevice device, StreemaSettings settings)
            : base(device)
        {
            _settings = settings;
            _urlParameters = new List<Parameter>();
        }

        #endregion

        #region Init

        protected override async void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter(0x6040, 0x00) as Parameter;
            _statusWordParameter = Vcs.SearchParameter(0x6041, 0x00) as Parameter;
            
            _activeStationParameter = Vcs.SearchParameter(0x4001, 0x00) as Parameter;
            _volumeParameter = Vcs.SearchParameter(0x4002, 0x00) as Parameter;

            var maxUrlsCountParameter = Vcs.SearchParameter(0x4000, 0x00) as Parameter;

            if(maxUrlsCountParameter != null && maxUrlsCountParameter.GetValue(out byte maxCount))
            {
                for(byte i = 0; i < maxCount; i++)
                {
                    var urlParameter = Vcs.SearchParameter(0x4000, (byte)(i + 1)) as Parameter;

                    if (urlParameter != null)
                    {
                        _urlParameters.Add(urlParameter);
                    }
                }                
            }

            if(_activeStationParameter != null)
            {
                _activeStationParameter.ParameterChanged += OnActiveStationParameterChanged;

                await _activeStationParameter.UpdateValue();

                SetActiveStationAsync(_activeStationParameter);
            }

            if (_volumeParameter != null)
            {
                _volumeParameter.ParameterChanged += OnVolumeChanged;

                await _volumeParameter.UpdateValue();

                SetVolumeAsync(_activeStationParameter);
            }

            base.OnInitialized();
        }

        #endregion

        #region Events

        private void OnActiveStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterValue = e.NewValue;
            int activeStationValue = 0;

            if(parameterValue.GetValue(ref activeStationValue))
            {
                Console.WriteLine($"Active Station Changed = {activeStationValue}");

                SetActiveStationAsync(activeStationValue);
            }
        }
        private void OnVolumeChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterValue = e.NewValue;
            int currentValue = 0;

            if (parameterValue.GetValue(ref currentValue))
            {
                Console.WriteLine($"Volume Changed = {currentValue}");

                SetVolumeAsync(currentValue);
            }
        }

        #endregion

            #region SDO

        public override bool GetObject(ushort objectIndex, byte objectSubindex, ref byte[] data)
        {
            bool result = false;

            //PARAM_ControlWord
            if (objectIndex == 0x6040 && objectSubindex == 0x0)
            {
                if(_controlWordParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x6041)
            {
                if (_statusWordParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            } 
            else if (objectIndex == 0x4001)
            {
                if (_activeStationParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x4002)
            {
                if (_volumeParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex == 0x4000)
            {
                if (_urlParameters.Count > objectSubindex)
                {
                    if (_urlParameters[objectSubindex - 1].GetValue(out byte[] d1))
                    {
                        data = d1;
                        result = true;
                    }
                }
            }

            return result;
        }

        public override bool SetObject(ushort objectIndex, byte objectSubindex, byte[] data)
        {
            bool result = false;

            //PARAM_ControlWord
            if (objectIndex == 0x6040 && objectSubindex == 0x0)
            {
                var controlWordValue = BitConverter.ToUInt16(data, 0);

                Console.WriteLine($"new controlword value = {controlWordValue}");

                result = _controlWordParameter.SetValue(controlWordValue);
            }
            else if (objectIndex == 0x4000)
            {
                if (_urlParameters.Count > objectSubindex)
                {
                    result = _urlParameters[objectSubindex - 1].SetValue(data);
                }
            }
            else if (objectIndex == 0x4001 && objectSubindex == 0x0)
            {
                var activeStationValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new active station value = {activeStationValue}");

                result = _activeStationParameter.SetValue(activeStationValue);
            }
            else if (objectIndex == 0x4002)
            {
                var volumeValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new volume value = {volumeValue}");

                result = _volumeParameter.SetValue(volumeValue);
            }

            return result;
        }

        #endregion

        #region Events

        protected override void OnStatusChanged(DeviceCommunicationEventArgs e)
        {
            Console.WriteLine($"status changed, status = {e.Device.Status}, error code = {e.LastErrorCode}");

            base.OnStatusChanged(e);
        }

        private void SetActiveStationAsync(int activeStationValue)
        {
            Task.Run(() =>
            {
                if (_urlParameters.Count > activeStationValue)
                {
                    var urlParam = _urlParameters[activeStationValue];

                    if (urlParam.GetValue(out string url))
                    {
                        foreach (var p in Process.GetProcessesByName(_settings.AppName))
                        {
                            p.Kill();
                        }

                        var startInfo = new ProcessStartInfo(_settings.AppPath);

                        startInfo.WindowStyle = ProcessWindowStyle.Normal;

                        string playUrl = _settings.PlayUrl;

                        if(!playUrl.EndsWith('/'))
                        {
                            playUrl += "/";
                        }

                        startInfo.Arguments = _settings.AppArgs + $" {playUrl}{url}";

                        Process.Start(startInfo);
                    }
                }
            });
        }

        private void SetVolumeAsync(int volumeValue)
        {
            Task.Run(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string args = $"-D pulse sset Master {volumeValue}%";

                    var startInfo = new ProcessStartInfo("amixer");

                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = args;

                    Process.Start(startInfo);
                }
            });
        }

        private void SetActiveStationAsync(Parameter activeStation)
        {
            if (activeStation != null)
            {
                if (activeStation.GetValue(out int activeStationValue))
                {
                    SetActiveStationAsync(activeStationValue);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - SetActiveStationAsync", "get activeStation parameter value failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - SetActiveStationAsync", "activeStation parameter not defined!");
            }
        }
        private void SetVolumeAsync(Parameter parameter)
        {
            if (parameter != null)
            {
                if (parameter.GetValue(out int parameterValue))
                {
                    SetVolumeAsync(parameterValue);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - SetVolumeAsync", "get volume parameter value failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - SetVolumeAsync", "volume parameter not defined!");
            }
        }

        #endregion
    }
}
