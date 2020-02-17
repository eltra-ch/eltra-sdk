/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using EltraCommon.Logger;

#pragma warning disable CS1591

namespace EltraCloud.Ip2Location
{
    public class Db5Parser
    {
        #region Private fields

        private List<Db5Entry> _db5Entries;

        #endregion

        #region Constructors

        public Db5Parser()
        {
            _db5Entries = new List<Db5Entry>();
        }

        #endregion

        #region Properties

        public bool HasEntries => _db5Entries.Count > 0;

        #endregion

        #region Methods

        #region Public

        private Db5Entry FindAddress(long ipNumber)
        {
            Db5Entry result = null;

            try
            {
                if (HasEntries)
                {
                    var index = BinarySearch(ipNumber);

                    if (index != -1)
                    {
                        result = _db5Entries[index];
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - FindAddress", e);
            }

            return result;
        }

        public Db5Entry FindAddress(IPAddress address)
        {
            Db5Entry result = null;

            if (HasEntries)
            {
                IPAddress ipv4Address;

                if (address.IsIPv4MappedToIPv6)
                {
                    ipv4Address = address.MapToIPv4();
                }
                else
                {
                    ipv4Address = address;
                }

                long ipNumber = GetIpv4AsNumber(ipv4Address.GetAddressBytes());

                result = FindAddress(ipNumber);
            }

            return result;
        }

        public bool Load(string fileName)
        {
            bool result = false;
            CultureInfo englishCulture = new CultureInfo("en");

            try
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        _db5Entries = new List<Db5Entry>();

                        result = true;

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                ChangeDelimiterToSemicolon(ref line);

                                var values = line.Split(';');

                                try
                                {
                                    NormalizeEntryElements(ref values);

                                    var db5Entry = new Db5Entry
                                                   {
                                                       CountryCode = values[2].Trim(),
                                                       Country = values[3].Trim(),
                                                       Region = values[4].Trim(),
                                                       City = values[5].Trim(),
                                                       From = Convert.ToUInt32(values[0]),
                                                       To = Convert.ToUInt32(values[1])
                                                   };


                                    if (double.TryParse(values[6], NumberStyles.Float, englishCulture, out var latitude))
                                    {
                                        db5Entry.Latitude = latitude;
                                    }
                                    else
                                    {
                                        result = false;
                                        break;
                                    }

                                    if (double.TryParse(values[7], NumberStyles.Float, englishCulture, out var longitude))
                                    {
                                        db5Entry.Longitude = longitude;
                                    }
                                    else
                                    {
                                        result = false;
                                        break;
                                    }

                                    _db5Entries.Add(db5Entry);
                                }
                                catch (Exception)
                                {
                                    result = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Load", e);
            }

            return result;
        }

        #endregion

        #region Internal

        private Db5EntryRange IsInRange(Db5Entry dbEntry, long ipNumber)
        {
            var result = IsInRange(dbEntry.From, dbEntry.To, ipNumber);

            return result;
        }

        private Db5EntryRange IsInRange(long from, long to, long ipNumber)
        {
            Db5EntryRange result = Db5EntryRange.Lower;

            if (ipNumber >= from && ipNumber <= to)
            {
                result = Db5EntryRange.InRange;
            }
            else if (ipNumber > to)
            {
                result = Db5EntryRange.Higher;
            }
            else if (ipNumber < from)
            {
                result = Db5EntryRange.Lower;
            }

            return result;
        }

        private int BinarySearch(long ipNumber)
        {
            int low = 0; // 0 is always going to be the first element
            int high = _db5Entries.Count - 1; // Find highest element
            int middle = (low + high + 1) / 2; // Find middle element
            int location = -1; // Return value -1 if not found

            do // Search for element
            {
                // if element is found at middle
                if (IsInRange(_db5Entries[middle], ipNumber) == Db5EntryRange.InRange)
                    location = middle; // location is current middle

                // middle element is too high
                else if (IsInRange(_db5Entries[middle], ipNumber) == Db5EntryRange.Lower)
                    high = middle - 1; // eliminate lower half
                else // middle element is too low
                    low = middle + 1; // eleminate lower half

                middle = (low + high + 1) / 2; // recalculate the middle  
            } while ((low <= high) && (location == -1));

            return location; // return location of search key
        }

        private uint GetIpv4AsNumber(byte[] ipv4Address)
        {
            return GetIpv4AsNumber(ipv4Address[0], ipv4Address[1], ipv4Address[2], ipv4Address[3]);
        }

        private uint GetIpv4AsNumber(byte w, byte x, byte y, byte z)
        {
            uint ipNumber = (uint)(16777216 * w + 65536 * x + 256 * y + z);

            return ipNumber;
        }

        private static void NormalizeEntryElements(ref string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Replace("\"", "").Trim();
            }
        }

        private static void ChangeDelimiterToSemicolon(ref string text)
        {
            var textArray = text.ToCharArray();

            for (int i = 0; i < textArray.Length; i++)
            {
                if (i - 1 > 0 && i + 1 < textArray.Length)
                {
                    if (i > 1 && i < textArray.Length - 1)
                    {
                        if (textArray[i] == ',' && textArray[i - 1] == '"' && textArray[i + 1] == '"')
                        {
                            textArray[i] = ';';
                        }
                        else if (textArray[i] == ',' && textArray[i - 1] == '"' && textArray[i + 1] == 0)
                        {
                            textArray[i] = ';';
                        }
                        else if (textArray[i] == ',' && textArray[i - 1] == 0 && textArray[i + 1] == '"')
                        {
                            textArray[i] = ';';
                        }
                    }
                }
            }

            text = new string(textArray);
        }

        #endregion

        #endregion
    }
}
