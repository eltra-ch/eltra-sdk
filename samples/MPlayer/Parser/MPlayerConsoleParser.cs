using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPlayerMaster
{
    class MPlayerConsoleParser
    {
        #region Properties

        public Parameter ActiveStationParameter { get; set; }
        public List<Parameter> StreamTitleParameters { get; set; }
        public List<Parameter> StationTitleParameters { get; set; }

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
        
        #endregion

        private static string RemoveWhitespace(string input)
        {
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(input))
            {
                result = new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
            }

            return result;
        }

        public void ProcessLine(string line)
        {
            try
            {
                var formattedLine = RemoveWhitespace(line).ToLower();

                if (formattedLine.StartsWith("name:"))
                {
                    ParseMPlayerStationName(line);
                }
                else if (formattedLine.StartsWith("icyinfo:streamtitle"))
                {
                    ParseMPlayerStreamTitle(line);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ParseMPlayerOutputLine", e);
            }
        }

        private void ParseMPlayerStreamTitle(string line)
        {
            try
            {
                int sep = line.IndexOf('=');

                if (sep >= 0 && line.Length > sep + 2)
                {
                    string streamTitle = line.Substring(sep + 2).Trim();

                    if (!string.IsNullOrEmpty(streamTitle))
                    {
                        int end = streamTitle.IndexOf('\'');
                        if (end >= 0 && streamTitle.Length > end)
                        {
                            streamTitle = streamTitle.Substring(0, end);

                            if (!string.IsNullOrEmpty(streamTitle))
                            {
                                Console.WriteLine($"stream title: {streamTitle}");

                                if (ActiveStationValue > 0)
                                {
                                    int index = ActiveStationValue - 1;

                                    if (!StreamTitleParameters[index].SetValue(streamTitle))
                                    {
                                        MsgLogger.WriteError($"{GetType().Name} - ParseMPlayerStationName", $"cannot set new stream title: {streamTitle}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ParseMPlayerStreamTitle", e);
            }
        }

        private void ParseMPlayerStationName(string line)
        {
            try
            {
                int sep = line.IndexOf(':');

                if (sep >= 0 && line.Length > sep + 1)
                {
                    string stationName = line.Substring(sep + 1).Trim();

                    if (!string.IsNullOrEmpty(stationName))
                    {
                        Console.WriteLine($"station name: {stationName}");

                        if (ActiveStationValue > 0)
                        {
                            int index = ActiveStationValue - 1;

                            if (!StationTitleParameters[index].SetValue(stationName))
                            {
                                MsgLogger.WriteError($"{GetType().Name} - ParseMPlayerStationName", $"cannot set new station name: {stationName}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ParseMPlayerStationName", e);
            }
        }
    }
}
