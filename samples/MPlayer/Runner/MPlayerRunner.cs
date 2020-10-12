using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static MPlayerMaster.MPlayerDefinitions;

namespace MPlayerMaster
{
    internal class MPlayerRunner
    {
        #region Private fields

        private readonly string _mplayerProcessName;

        private MPlayerConsoleParser _parser;

        #endregion

        #region Constructors

        public MPlayerRunner()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _mplayerProcessName = "/usr/bin/mplayer";
            }
            else
            {
                _mplayerProcessName = "C:\\MPlayer\\mplayer.exe";
            }
        }

        #endregion

        #region Properties

        public int ActiveStationValue
        {
            get
            {
                int result = -1;

                if (ActiveStationParameter != null && ActiveStationParameter.GetValue(out int activeStationValue))
                {
                    result = activeStationValue;
                }

                return result;
            }
        }

        public List<Parameter> StationTitleParameters { get; set; }
        public List<Parameter> ProcessIdParameters { get; set; }
        public List<Parameter> StreamTitleParameters { get; set; }

        public Parameter StationsCountParameter { get; set; }
        public Parameter StatusWordParameter { get; set; }
        public Parameter ActiveStationParameter { get; set; }

        public MPlayerSettings Settings;

        internal MPlayerConsoleParser Parser => _parser ?? (_parser = CreateParser());

        #endregion

        #region Methods

        private MPlayerConsoleParser CreateParser()
        {
            var result = new MPlayerConsoleParser
            {
                ActiveStationParameter = ActiveStationParameter,
                StreamTitleParameters = StreamTitleParameters,
                StationTitleParameters = StationTitleParameters
            };

            return result;
        }

        private bool SetExecutionStatus(StatusWordEnums status)
        {
            bool result = false;

            if (StatusWordParameter != null)
            {
                result = StatusWordParameter.SetValue((ushort)status);
            }

            return result;
        }

        public void Start(Parameter processParam, string url)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var startInfo = new ProcessStartInfo();
                var p = new Process();

                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;

                startInfo.WindowStyle = ProcessWindowStyle.Normal;

                startInfo.Arguments = Settings.AppArgs + $" {url}";
                startInfo.Arguments = startInfo.Arguments.Trim();
                startInfo.FileName = _mplayerProcessName;

                SetExecutionStatus(StatusWordEnums.PendingExecution);

                p.StartInfo = startInfo;

                p.OutputDataReceived += (sender, args) =>
                {
                    Parser.ProcessLine(args.Data);
                };

                p.Start();

                p.BeginOutputReadLine();

                SetExecutionStatus(p != null ? StatusWordEnums.ExecutedSuccessfully : StatusWordEnums.ExecutionFailed);

                if (p != null)
                {
                    if (!processParam.SetValue(p.Id))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetActiveStationAsync", "process id cannot be set");
                    }
                }

                MsgLogger.WriteFlow($"{GetType().Name} - SetActiveStationAsync", $"Set Station request: {url}, result = {p != null}");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SetActiveStationAsync", e);
            }
        }

        public void Stop()
        {
            try
            {
                bool gracefullClose = false;

                if (StationsCountParameter.GetValue(out ushort maxCount))
                {
                    for (ushort i = 0; i < maxCount; i++)
                    {
                        if (ProcessIdParameters[i].GetValue(out int processId) && processId > 0)
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
                            catch (Exception e)
                            {
                                MsgLogger.Exception($"{GetType().Name} - CloseWebAppInstances [1]", e);
                            }
                        }
                    }
                }

                if (!gracefullClose)
                {
                    MsgLogger.WriteFlow("close browser brute force");

                    foreach (var p in Process.GetProcessesByName(_mplayerProcessName))
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CloseWebAppInstances [2]", e);
            }
        }

        #endregion
    }
}
