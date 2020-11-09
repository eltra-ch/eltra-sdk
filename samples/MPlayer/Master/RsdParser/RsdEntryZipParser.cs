using EltraCommon.Helpers;
using EltraCommon.Logger;
using MPlayerCommon.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace MPlayerMaster.RsdParser
{
    class RsdEntryZipParser
    {
        public bool GetRsdEntries(StringReader reader, out List<RadioStationEntry> radioSureEntries)
        {
            bool result = false;
            string line;
            
            radioSureEntries = new List<RadioStationEntry>();

            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(new char[] { '\t' });
                
                var radioSureEntry = ProcessColumn(columns);

                if (radioSureEntry != null)
                {
                    radioSureEntries.Add(radioSureEntry);
                    result = true;
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - GetRsdEntries", "parsing rsd entry!");
                }
            }

            return result;
        }

        private RadioStationEntry ProcessColumn(string[] columns)
        {
            int count = 0;
            var radioSureEntry = new RadioStationEntry();
            
            foreach (var column in columns)
            {
                FillRadioSureEntry(radioSureEntry, count, columns[count]);
                count++;
            }

            return radioSureEntry;
        }

        private static RadioStationEntry FillRadioSureEntry(RadioStationEntry radioSureEntry, int count, string columnValue)
        {
            if (columnValue == "-")
            {
                columnValue = string.Empty;
            }

            switch (count)
            {
                case 0:
                    radioSureEntry.Name = columnValue;
                    break;
                case 1:
                    radioSureEntry.Description = columnValue;
                    break;
                case 2:
                    radioSureEntry.Genre = columnValue;
                    break;
                case 3:
                    radioSureEntry.Country = columnValue;
                    break;
                case 4:
                    radioSureEntry.Language = columnValue;
                    break;
            }

            if (count >= 5 && !string.IsNullOrEmpty(columnValue))
            {
                Uri uriResult;
                bool validUri = Uri.TryCreate(columnValue, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (validUri)
                {
                    radioSureEntry.Urls.Add(columnValue);
                }
            }

            return radioSureEntry;
        }
    }
}
