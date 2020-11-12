using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraMaster.DeviceManager.Events;
using System;
using EltraConnector.Master.Device;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using System.Runtime.InteropServices;

using static MPlayerMaster.MPlayerDefinitions;
using System.IO;
using MPlayerMaster.RsdParser;
using MPlayerCommon.Contracts;

namespace MPlayerMaster.Device
{
    public class MPlayerDeviceCommunication : MasterDeviceCommunication
    {
        #region Private fields

        private readonly List<Parameter> _urlParameters;
        private readonly List<Parameter> _stationTitleParameters;
        private readonly List<Parameter> _streamTitleParameters;
        private readonly List<Parameter> _volumeScalingParameters;
        private readonly List<Parameter> _processIdParameters;

        private Parameter _activeStationParameter;
        private Parameter _volumeParameter;
        private XddParameter _muteParameter;
        private Parameter _statusWordParameter;

        private Parameter _controlWordParameter;
        private Parameter _stationsCountParameter;

        private List<RadioStationEntry> _radioStations;
        private MPlayerSettings _settings;
        private ushort _maxStationsCount;

        private MPlayerRunner _runner;

        #endregion

        #region Constructors

        public MPlayerDeviceCommunication(MasterDevice device, MPlayerSettings settings)
            : base(device)
        {
            _settings = settings;

            _urlParameters = new List<Parameter>();
            _stationTitleParameters = new List<Parameter>();
            _streamTitleParameters = new List<Parameter>();
            _volumeScalingParameters = new List<Parameter>();
            _processIdParameters = new List<Parameter>();
            _radioStations = new List<RadioStationEntry>();
        }

        #endregion

        #region Properties

        internal MPlayerRunner Runner => _runner ?? (_runner = CreateRunner());

        public int ActiveStationValue
        {
            get
            {
                int result = -1;

                if(_activeStationParameter != null && _activeStationParameter.GetValue(out int activeStationValue))
                {
                    result = activeStationValue;
                }

                return result;
            }
        }

        #endregion

        #region Init

        private MPlayerRunner CreateRunner()
        {
            var result = new MPlayerRunner
            {
                StationTitleParameters = _stationTitleParameters,
                ProcessIdParameters = _processIdParameters,
                StreamTitleParameters = _streamTitleParameters,

                StationsCountParameter = _stationsCountParameter,
                StatusWordParameter = _statusWordParameter,
                ActiveStationParameter = _activeStationParameter,

                Settings = _settings
            };

            ReadRadioStations();

            return result;
        }

        private void ReadRadioStations()
        {
            if(File.Exists(_settings.RsdZipFile))
            {
                var parser = new RsdFileParser() { SerializeToJsonFile = false };

                if(parser.ConvertRsdZipFileToJson(_settings.RsdZipFile))
                {
                    _radioStations = parser.Output;
                }
            }
        }

        protected override async void OnInitialized()
        {
            Console.WriteLine($"device (node id={Device.NodeId}) initialized, processing ...");
            
            InitStateMachine();

            InitializeStationList();

            await SetActiveStation();

            await InitVolumeControl();

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
                _maxStationsCount = maxCount;

                for (ushort i = 0; i < maxCount; i++)
                {
                    ushort index = (ushort)(0x4000 + i);

                    var urlParameter = Vcs.SearchParameter(index, 0x01) as XddParameter;
                    var stationTitleParameter = Vcs.SearchParameter(index, 0x02) as XddParameter;
                    var streamTitleParameter = Vcs.SearchParameter(index, 0x03) as XddParameter;
                    var valumeScalingParameter = Vcs.SearchParameter(index, 0x04) as XddParameter;
                    var processIdParameter = Vcs.SearchParameter(index, 0x05) as XddParameter;

                    if (urlParameter != null && 
                        stationTitleParameter != null && 
                        streamTitleParameter != null && 
                        valumeScalingParameter != null && 
                        processIdParameter != null)
                    {
                        _urlParameters.Add(urlParameter);
                        _stationTitleParameters.Add(stationTitleParameter);
                        _streamTitleParameters.Add(streamTitleParameter);
                        _volumeScalingParameters.Add(valumeScalingParameter);
                        _processIdParameters.Add(processIdParameter);
                    }
                }
            }
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
            else if (objectIndex == 0x4100)
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
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount) && objectSubindex == 0x01
                      && _urlParameters.Count > 0)
            {
                if (_urlParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
                {
                    data = d1;
                    result = true;
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount) && objectSubindex == 0x02
                      && _stationTitleParameters.Count > 0)
            {
                if (_stationTitleParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
                {
                    data = d1;
                    result = true;
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount) && objectSubindex == 0x03
                      && _streamTitleParameters.Count > 0)
            {
                if (_streamTitleParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
                {
                    data = d1;
                    result = true;
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount) && objectSubindex == 0x04
                      && _volumeScalingParameters.Count > 0)
            {
                if (_volumeScalingParameters[objectIndex - 0x4000].GetValue(out byte[] d1))
                {
                    data = d1;
                    result = true;
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount) && objectSubindex == 0x05
                      && _processIdParameters.Count > 0)
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
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount)
                    && objectSubindex == 0x01)
            {
                if (_urlParameters.Count > (objectIndex - 0x4000))
                {
                    result = _urlParameters[objectIndex - 0x4000].SetValue(data);
                }
            }
            else if (objectIndex >= 0x4000 && objectIndex <= (0x4000 + _maxStationsCount)
                    && objectSubindex == 0x04)
            {
                if (_volumeScalingParameters.Count > (objectIndex - 0x4000))
                {
                    result = _volumeScalingParameters[objectIndex - 0x4000].SetValue(data);
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

        private void SetEmptyStreamLabel(ushort stationIndex)
        {
            if (_streamTitleParameters.Count > stationIndex)
            {
                var streamParam = _streamTitleParameters[stationIndex];

                if (streamParam != null)
                {
                    streamParam.SetValue("-");
                }
            }
        }

        private Task SetActiveStationAsync(int activeStationValue)
        {
            var result = Task.Run(() =>
            {
                if(activeStationValue == 0)
                {
                    for (ushort i = 0; i < _maxStationsCount; i++)
                    {
                        SetEmptyStreamLabel(i);
                    }

                    SetExecutionStatus(StatusWordEnums.PendingExecution);

                    bool result = Runner.Stop();

                    SetExecutionStatus(result ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);
                }
                else if (_urlParameters.Count >= activeStationValue && activeStationValue > 0)
                {
                    var urlParam = _urlParameters[activeStationValue - 1];
                    var processParam = _processIdParameters[activeStationValue - 1];
                    
                    if (urlParam.GetValue(out string url))
                    {
                        SetExecutionStatus(StatusWordEnums.PendingExecution);

                        SetEmptyStreamLabel((ushort)(activeStationValue - 1));

                        Runner.Stop();

                        var result = Runner.Start(processParam, url);

                        SetExecutionStatus(result ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);
                    }
                }
            });

            return result;
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

                        var startInfo = new ProcessStartInfo("amixer")
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = args
                        };

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

        public string QueryStation(string query)
        {
            string result = string.Empty;
            const int minQueryLength = 3;
            const int maxRadioStationEntries = 25;

            try
            {
                if (_radioStations != null && _radioStations.Count > 0 && query.Length > minQueryLength)
                {
                    var radioStations = new List<RadioStationEntry>();

                    var queryWords = query.Split(new char[] { ' ', ';', ';' });

                    if (queryWords.Length > 0)
                    {
                        foreach (var queryWord in queryWords)
                        {
                            var lowQueryWord = queryWord.ToLower();

                            foreach (var radioStation in _radioStations)
                            {
                                if (radioStation.Name.ToLower().Contains(lowQueryWord) ||
                                    radioStation.Genre.ToLower().Contains(lowQueryWord) ||
                                    radioStation.Country.ToLower().Contains(lowQueryWord) ||
                                    radioStation.Language.ToLower().Contains(lowQueryWord))
                                {
                                    radioStations.Add(radioStation);

                                    if(radioStations.Count > maxRadioStationEntries)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (radioStations.Count > maxRadioStationEntries)
                            {
                                break;
                            }
                        }
                    }

                    result = System.Text.Json.JsonSerializer.Serialize(radioStations);
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - QueryStation", e);
            }

            return result;
        }

        #endregion
    }
}
