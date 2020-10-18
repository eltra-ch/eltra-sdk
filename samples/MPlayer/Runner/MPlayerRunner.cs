﻿using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

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

        public bool Start(Parameter processParam, string url)
        {
            bool result = false;

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

                p.StartInfo = startInfo;

                p.OutputDataReceived += (sender, args) =>
                {
                    Parser.ProcessLine(args.Data);
                };

                p.Start();

                p.BeginOutputReadLine();

                if (p != null)
                {
                    if (!processParam.SetValue(p.Id))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - SetActiveStationAsync", "process id cannot be set");
                    }
                    else
                    {
                        result = true;
                    }
                }

                MsgLogger.WriteFlow($"{GetType().Name} - SetActiveStationAsync", $"Set Station request: {url}, result = {p != null}");
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SetActiveStationAsync", e);
            }

            return result;
        }

        public bool Stop()
        {
            bool result = false;

            try
            {
                result = CloseActualStationProcess();

                if (!result)
                {
                    result = TryCloseAllProcesses();
                }

                if (!result)
                {
                    CloseBruteForce();
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Stop [2]", e);
            }

            return result;
        }

        private bool TryCloseAllProcesses()
        {
            bool result = false;

            if (StationsCountParameter.GetValue(out ushort maxCount))
            {
                for (ushort i = 0; i < maxCount; i++)
                {
                    if (ProcessIdParameters[i].GetValue(out int processId) && processId > 0)
                    {
                        result = CloseProcess(processId);
                    }
                }
            }

            return result;
        }

        private bool CloseActualStationProcess()
        {
            bool result = false;
            int i = ActiveStationValue - 1;

            if (i >= 0)
            {
                if (ProcessIdParameters[i].GetValue(out int processId) && processId > 0)
                {
                    result = CloseProcess(processId);
                }
            }

            return result;
        }

        private void CloseBruteForce()
        {
            MsgLogger.WriteFlow($"close '{_mplayerProcessName}' brute force");

            foreach (var p in Process.GetProcessesByName(_mplayerProcessName))
            {
                p.Kill();
            }
        }

        private bool CloseProcess(int processId)
        {
            bool result = false;
            
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

                        result = p.WaitForExit(MaxWaitTimeInMs);
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Stop", $"process id exited - pid {processId}");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Stop", $"process id not found - pid {processId}");
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Stop [1]", e);
            }

            return result;
        }

        #endregion
    }
}