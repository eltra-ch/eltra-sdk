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
using System.Timers;

namespace StreemaMaster
{
    public class StreemaDeviceCommunication : MasterDeviceCommunication
    {
        #region Enums

        internal enum StatusWordEnums
        {
            Undefined = 0x0,
            Waiting = 0x0001,
            PendingExecution = 0x0010,
            ExecutedSuccessfully = 0x0020,
            ExecutionFailed = 0x8000
        }

        #endregion

        #region Private fields

        private readonly List<Parameter> _urlParameters;
        private readonly List<Parameter> _labelParameters;
        private readonly List<Parameter> _imageParameters;
        private readonly List<Parameter> _volumeScalingParameters;
        private readonly List<Parameter> _processIdParameters;

        private Parameter _activeStationParameter;
        private Parameter _volumeParameter;
        private XddParameter _muteParameter;
        private Parameter _statusWordParameter;
        private Parameter _controlWordParameter;
        private Parameter _stationsCountParameter;
        private StreemaSettings _settings;

        private Timer _stationInfoUpdateTimer;

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
            _processIdParameters = new List<Parameter>();
        }

        #endregion

        #region Init

        protected override async void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");
            
            InitStateMachine();

            InitializeStationList();

            await SetActiveStation();

            await InitVolumeControl();

            if (!await UpdateLabels())
            {
                StartStationInfoUpdate();
            }

            base.OnInitialized();
        }

        private void InitStateMachine()
        {
            _controlWordParameter = Vcs.SearchParameter("PARAM_ControlWord") as XddParameter;
            _statusWordParameter = Vcs.SearchParameter("PARAM_StatusWord") as XddParameter;

            if(!SetExecutionStatus(StatusWordEnums.Waiting))
            {
                MsgLogger.WriteError($"{GetType().Name} - InitStateMachine", "Set execution state (waiting) failed!");
            }
        }

        private async Task InitVolumeControl()
        {
            _muteParameter = Vcs.SearchParameter("PARAM_Mute") as XddParameter;
            if (_muteParameter != null)
            {
                _muteParameter.ParameterChanged += OnMuteChanged;

                await _muteParameter.UpdateValue();

                await SetMuteAsync(_muteParameter);
            }

            _volumeParameter = Vcs.SearchParameter("PARAM_Volume") as Parameter;

            if (_volumeParameter != null)
            {
                _volumeParameter.ParameterChanged += OnVolumeChanged;

                await _volumeParameter.UpdateValue();

                await SetVolumeAsync(_volumeParameter);
            }
        }

        private async Task SetActiveStation()
        {
            _activeStationParameter = Vcs.SearchParameter("PARAM_ActiveStation") as Parameter;
            
            if (_activeStationParameter != null)
            {
                _activeStationParameter.ParameterChanged += OnActiveStationParameterChanged;

                await _activeStationParameter.UpdateValue();

                await SetActiveStationAsync(_activeStationParameter);
            }
        }

        private void InitializeStationList()
        {
            _stationsCountParameter = Vcs.SearchParameter("PARAM_StationsCount") as XddParameter;

            if (_stationsCountParameter != null && _stationsCountParameter.GetValue(out ushort maxCount))
            {
                for (ushort i = 0; i < maxCount; i++)
                {
                    ushort index = (ushort)(0x4000 + i);

                    var urlParameter = Vcs.SearchParameter(index, 0x01) as XddParameter;
                    var labelParameter = Vcs.SearchParameter(index, 0x02) as XddParameter;
                    var imageParameter = Vcs.SearchParameter(index, 0x03) as XddParameter;
                    var valumeScalingParameter = Vcs.SearchParameter(index, 0x04) as XddParameter;
                    var processIdParameter = Vcs.SearchParameter(index, 0x05) as XddParameter;

                    if (urlParameter != null && labelParameter != null && 
                        imageParameter != null && valumeScalingParameter != null && processIdParameter != null)
                    {
                        urlParameter.ParameterChanged += OnUrlParameterChanged;

                        _urlParameters.Add(urlParameter);
                        _labelParameters.Add(labelParameter);
                        _imageParameters.Add(imageParameter);
                        _volumeScalingParameters.Add(valumeScalingParameter);
                        _processIdParameters.Add(processIdParameter);
                    }
                }
            }
        }

        private void StartStationInfoUpdate()
        {
            if(_stationInfoUpdateTimer!=null)
            {
                _stationInfoUpdateTimer.Stop();
            }

            _stationInfoUpdateTimer = new Timer(60000);
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

        private void OnMuteChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameterValue = e.NewValue;
            bool currentValue = false;

            if (parameterValue.GetValue(ref currentValue))
            {
                Console.WriteLine($"Mute Changed = {currentValue}");

                SetMuteAsync(currentValue);
            }
        }

        #endregion

        private Task<bool> UpdateLabel(Parameter urlParameter)
        {
            var t = Task.Run(() =>
            {
                bool result = false;
                try
                {
                    if (urlParameter.GetValue(out string url))
                    {
                        var site = new SiteProcessor(_settings.PlayUrl + url);

                        if (site.Parse())
                        {
                            var titleMeta = site.FindMetaTagByPropertyName("og:title");

                            ushort i = (ushort)(urlParameter.Index - 0x4000);

                            if (titleMeta != null)
                            {
                                titleMeta.Content = titleMeta.Content.Trim();
                                if (!string.IsNullOrEmpty(titleMeta.Content))
                                {
                                    _labelParameters[i].SetValue(titleMeta.Content);
                                    result = true;
                                }
                            }

                            if (result)
                            {
                                var imageUrlMeta = site.FindMetaTagByPropertyName("og:image");

                                if (imageUrlMeta != null)
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
                    }                   
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - UpdateLabel", e);
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
            else if (objectIndex == 0x4201)
            {
                if (_muteParameter.GetValue(out byte[] v))
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
            else if (objectIndex >= 0x4000 && objectIndex <= 0x4003 && objectSubindex == 0x05
                      && _urlParameters.Count > 0)
            {
                if (_processIdParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
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
            else if (objectIndex == 0x4201)
            {
                var muteValue = BitConverter.ToBoolean(data, 0);

                Console.WriteLine($"new mute value = {muteValue}");

                result = _muteParameter.SetValue(muteValue);
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

        private Task SetActiveStationAsync(int activeStationValue)
        {
            var result = Task.Run(() =>
            {
                if (_urlParameters.Count > activeStationValue)
                {
                    var urlParam = _urlParameters[activeStationValue];
                    var processParam = _processIdParameters[activeStationValue];

                    if (urlParam.GetValue(out string url))
                    {
                        CloseWebAppInstances();

                        try
                        {
                            var startInfo = new ProcessStartInfo(_settings.AppPath);

                            startInfo.WindowStyle = ProcessWindowStyle.Normal;

                            string playUrl = _settings.PlayUrl;

                            startInfo.Arguments = _settings.AppArgs + $" {playUrl}{url}";

                            SetExecutionStatus(StatusWordEnums.PendingExecution);

                            var startResult = Process.Start(startInfo);

                            SetExecutionStatus(startResult != null ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);

                            if(startResult!=null)
                            {
                                if(!processParam.SetValue(startResult.Id))
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - SetActiveStationAsync", "process id cannot be set");
                                }
                            }

                            MsgLogger.WriteFlow($"{GetType().Name} - SetActiveStationAsync", $"Set Station request: {url}, result = {startResult != null}");
                        }
                        catch (Exception e)
                        {
                            MsgLogger.Exception($"{GetType().Name} - SetActiveStationAsync", e);
                        }
                    }
                }
            });

            return result;
        }

        private void CloseWebAppInstances()
        {
            try
            {
                bool gracefullClose = false;

                if(_stationsCountParameter.GetValue(out ushort maxCount))
                {
                    for(ushort i = 0; i < maxCount; i++)
                    {
                        if (_processIdParameters[i].GetValue(out int processId) && processId > 0)
                        {
                            try
                            {
                                var p = Process.GetProcessById(processId);

                                if (p != null)
                                {
                                    if (!p.HasExited)
                                    {
                                        const int MaxWaitTimeInMs = 10000;
                                        var startInfo = new ProcessStartInfo("kill");

                                        startInfo.WindowStyle = ProcessWindowStyle.Normal;
                                        startInfo.Arguments = $"{processId}";

                                        Process.Start(startInfo);

                                        gracefullClose = p.WaitForExit(MaxWaitTimeInMs);
                                    }
                                    else
                                    {
                                        MsgLogger.WriteError($"{GetType().Name} - CloseWebAppInstances", $"process id exited - pid {processId}");
                                    }
                                }
                                else
                                {
                                    MsgLogger.WriteError($"{GetType().Name} - CloseWebAppInstances", $"process id not found - pid {processId}");
                                }
                            }
                            catch(Exception e)
                            {
                                MsgLogger.Exception($"{GetType().Name} - CloseWebAppInstances [1]", e);
                            }
                        }
                    }
                }

                if (!gracefullClose)
                {
                    MsgLogger.WriteFlow("close browser brute force");

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
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CloseWebAppInstances [2]", e);
            }
        }

        private Task SetActiveStationAsync(Parameter activeStation)
        {
            Task result = null;

            if (activeStation != null)
            {
                if (activeStation.GetValue(out int activeStationValue))
                {
                    result = SetActiveStationAsync(activeStationValue);
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

            return result;
        }

        private Task SetVolumeAsync(Parameter parameter)
        {
            Task result = null;

            if (parameter != null)
            {
                if (parameter.GetValue(out int parameterValue))
                {
                    result = SetVolumeAsync(parameterValue);
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

            return result;
        }

        private Task SetVolumeAsync(int volumeValue)
        {
            var result = Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        string args = $"-D pulse sset Master {volumeValue}%";

                        var startInfo = new ProcessStartInfo("amixer");

                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = args;

                        SetExecutionStatus(StatusWordEnums.PendingExecution);

                        var startResult = Process.Start(startInfo);

                        SetExecutionStatus(startResult != null ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);

                        MsgLogger.WriteFlow($"{GetType().Name} - SetVolumeAsync", $"Set Volume request: {volumeValue}, result = {startResult != null}");
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - SetVolumeAsync", e);
                    }
                }
            });

            return result;
        }

        private Task SetMuteAsync(Parameter parameter)
        {
            Task result = null;
            if (parameter != null)
            {
                if (parameter.GetValue(out bool parameterValue))
                {
                    result = SetMuteAsync(parameterValue);
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

            return result;
        }

        private Task SetMuteAsync(bool muteValue)
        {
            var result = Task.Run(() =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        string muteString = muteValue ? "mute" : "unmute";

                        string args = $"-D pulse sset Master {muteString}";

                        var startInfo = new ProcessStartInfo("amixer");

                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = args;

                        SetExecutionStatus(StatusWordEnums.PendingExecution);

                        var startResult = Process.Start(startInfo);

                        MsgLogger.WriteFlow($"{GetType().Name} - SetMuteAsync", $"Mute request: {muteString}, result = {startResult!=null}");

                        SetExecutionStatus(startResult != null ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);
                    }
                    catch (Exception e)
                    {
                        MsgLogger.Exception($"{GetType().Name} - SetMuteAsync", e);
                    }
                }
            });

            return result;
        }

        private bool SetExecutionStatus(StatusWordEnums status)
        {
            bool result = false;

            if(_statusWordParameter!=null)
            {
                result = _statusWordParameter.SetValue((ushort)status);
            }

            return result;
        }

        #endregion
    }
}
