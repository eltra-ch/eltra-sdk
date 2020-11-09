using EltraCommon.Logger;
using MPlayerCommon.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MPlayerMaster.RsdParser
{
    class RsdFileParser
    {
        private List<RadioStationEntry> _output;

        public RsdFileParser()
        {
            SerializeToJsonFile = true;
        }

        public List<RadioStationEntry> Output => _output ?? (_output = new List<RadioStationEntry>());

        public bool SerializeToJsonFile { get; set; }

        public bool ConvertRsdZipFileToJson(string filePath)
        {
            bool result = false;

            try
            {
                using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open))
                {
                    result = ConvertZipArchiveToJson(zipToOpen);
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ConvertRsdZipFileToJson", e);
            }

            return result;
        }

        private bool ConvertZipArchiveToJson(FileStream zipToOpen)
        {
            bool result = false;

            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(".rsd"))
                    {
                        result = ConvertRsdFileToJson(entry, zipToOpen.Name);
                    }
                }
            }

            return result;
        }

        private bool ConvertRsdFileToJson(ZipArchiveEntry entry, string zipFileName)
        {
            bool result = false;

            try
            {
                var fileName = Path.ChangeExtension(zipFileName, "json");

                var processor = new RsdEntryZipParser();

                using (var stream = entry.Open())
                {
                    var streamReader = new StreamReader(stream);

                    result = processor.GetRsdEntries(new StringReader(streamReader.ReadToEnd()), out var entries);

                    if (result)
                    {
                        Output.AddRange(entries);

                        if (SerializeToJsonFile)
                        {
                            result = SerializeJsonFile(fileName, entries);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ConvertRsdFileToJson", e);
            }

            return result;
        }

        private bool SerializeJsonFile(string fileName, List<RadioStationEntry> entries)
        {
            bool result = false;

            try
            {
                if (entries.Count > 0)
                {
                    var jsonString = JsonSerializer.Serialize(entries);

                    File.WriteAllText(fileName, jsonString);

                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SerializeJsonFile", e);
            }

            return result;
        }
    }
}
