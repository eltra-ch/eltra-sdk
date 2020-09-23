using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraConnector.Master.Device;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using EltraCommon.Logger;
using StreemaMaster.Site;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace StreemaMaster
{
    public class StreemaDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly List<Parameter> _urlParameters;
        private readonly List<Parameter> _labelParameters;
        private readonly List<Parameter> _imageParameters;
        private readonly List<Parameter> _volumeScalingParameters;

        private Parameter _activeStationParameter;
        private Parameter _volumeParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        private Parameter _stationsCountParameter;
        private StreemaSettings _settings;

        private System.Timers.Timer _stationInfoUpdateTimer;

        #endregion

        #region Constructors

        public StreemaDeviceCommunication(MasterDevice device, StreemaSettings settings)
            : base(device)
        {
            _settings = settings;

            _urlParameters = new List<Parameter>();
            _labelParameters = new List<Parameter>();
            _imageParameters = new List<Parameter>();
            _volumeScalingParameters = new List<Parameter>();
        }

        #endregion

        #region Init

        protected override async void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");

            _controlWordParameter = Vcs.SearchParameter("PARAM_ControlWord") as XddParameter;
            _statusWordParameter = Vcs.SearchParameter("PARAM_StatusWord") as XddParameter;
            _stationsCountParameter = Vcs.SearchParameter("PARAM_StationsCount") as XddParameter;

            _activeStationParameter = Vcs.SearchParameter("PARAM_ActiveStation") as Parameter;
            _volumeParameter = Vcs.SearchParameter("PARAM_Volume") as Parameter;

            if (_stationsCountParameter != null && _stationsCountParameter.GetValue(out ushort maxCount))
            {
                for (ushort i = 0; i < maxCount; i++)
                {
                    ushort index = (ushort)(0x4000 + i);

                    var urlParameter = Vcs.SearchParameter(index, 0x01) as XddParameter;
                    var labelParameter = Vcs.SearchParameter(index, 0x02) as XddParameter;
                    var imageParameter = Vcs.SearchParameter(index, 0x03) as XddParameter;
                    var valumeScalingParameter = Vcs.SearchParameter(index, 0x04) as XddParameter;

                    if (urlParameter != null && labelParameter != null && imageParameter != null && valumeScalingParameter != null)
                    {
                        urlParameter.ParameterChanged += OnUrlParameterChanged;

                        _urlParameters.Add(urlParameter);
                        _labelParameters.Add(labelParameter);
                        _imageParameters.Add(imageParameter);
                        _volumeScalingParameters.Add(valumeScalingParameter);
                    }
                }
            }

            if (_activeStationParameter != null)
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

            if(!await UpdateLabels())
            {
                StartStationInfoUpdate();
            }
            
            base.OnInitialized();
        }

        private void StartStationInfoUpdate()
        {
            if(_stationInfoUpdateTimer!=null)
            {
                _stationInfoUpdateTimer.Stop();
            }

            _stationInfoUpdateTimer = new System.Timers.Timer(60000);
            _stationInfoUpdateTimer.Elapsed += OnStationInfoUpdate;
            _stationInfoUpdateTimer.Enabled = true;

            _stationInfoUpdateTimer.Start();
        }

        private async void OnStationInfoUpdate(object sender, ElapsedEventArgs e)
        {
            _stationInfoUpdateTimer.Stop();

            if (!await UpdateLabels())
            {
                StartStationInfoUpdate();
            }
        }

        private void OnUrlParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            Task.Run(async ()=> {

                if(!await UpdateLabel(e.Parameter))
                {
                    StartStationInfoUpdate();
                }            
            });            
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

        private Task<bool> UpdateLabel(Parameter urlParameter)
        {
            var t = Task.Run(() =>
            {
                bool result = false;

                if (urlParameter.GetValue(out string url))
                {
                    var site = new SiteProcessor(_settings.PlayUrl + url);

                    if (site.Parse())
                    {
                        var titleMeta = site.FindMetaTagByPropertyName("og:title");
                        var imageUrlMeta = site.FindMetaTagByPropertyName("og:image");

                        ushort i = (ushort)(urlParameter.Index - 0x4000);

                        titleMeta.Content = titleMeta.Content.Trim();
                        if (!string.IsNullOrEmpty(titleMeta.Content))
                        {
                            _labelParameters[i].SetValue(titleMeta.Content);
                            result = true;
                        }

                        if (result)
                        {
                            imageUrlMeta.Content = imageUrlMeta.Content.Trim();
                            if (!string.IsNullOrEmpty(imageUrlMeta.Content))
                            {
                                _imageParameters[i].SetValue(imageUrlMeta.Content);
                                result = true;
                            }
                        }
                    }
                }

                return result;
            });

            return t;
        }

        private async Task<bool> UpdateLabels()
        {
            bool result = true;

            foreach (var urlParameter in _urlParameters)
            {
                if(!await UpdateLabel(urlParameter))
                {
                    result = false;
                }
            }

            return result;
        }

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
            else if (objectIndex == 0x4200)
            {
                if (_volumeParameter.GetValue(out byte[] v))
                {
                    data = v;
                    result = true;
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= 0x4003 && objectSubindex == 0x01
                      && _urlParameters.Count > 0)
            {
                if (_urlParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
                {
                    data = d1;
                    result = true;
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
            else if (objectIndex >= 0x4000 && objectIndex <= 0x4003
                    && objectSubindex == 0x01)
            {
                if (_urlParameters.Count > (objectIndex - 0x4000))
                {
                    result = _urlParameters[objectIndex - 0x4000].SetValue(data);
                }
            }
            else if (objectIndex == 0x4100 && objectSubindex == 0x0)
            {
                var activeStationValue = BitConverter.ToInt32(data, 0);

                Console.WriteLine($"new active station value = {activeStationValue}");

                result = _activeStationParameter.SetValue(activeStationValue);
            }
            else if (objectIndex == 0x4200)
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
                        CloseWebAppInstances();

                        try
                        {
                            var startInfo = new ProcessStartInfo(_settings.AppPath);

                            startInfo.WindowStyle = ProcessWindowStyle.Normal;

                            string playUrl = _settings.PlayUrl;

                            startInfo.Arguments = _settings.AppArgs + $" {playUrl}{url}";

                            Process.Start(startInfo);
                        }
                        catch (Exception e)
                        {
                            MsgLogger.Exception($"{GetType().Name} - SetActiveStationAsync", e);
                        }
                    }
                }
            });
        }

        private void CloseWebAppInstances()
        {
            foreach (var p in Process.GetProcessesByName(_settings.AppName))
            {
                p.Kill();
            }

            if (_settings.IsWebKitProcess)
            {
                foreach (var p in Process.GetProcessesByName("WebKitWebProcess"))
                {
                    p.Kill();
                }
            }
        }

        private void SetVolumeAsync(int volumeValue)
        {
            Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        string args = $"-D pulse sset Master {volumeValue}%";

                        var startInfo = new ProcessStartInfo("amixer");

                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = args;

                        Process.Start(startInfo);
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - SetVolumeAsync", e);
                    }
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
