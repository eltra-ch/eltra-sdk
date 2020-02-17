using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters
{
    public class AllowedValues
    {
        private readonly DataTypeList _dataTypeList;
        private List<Epos4ValueEntry> _valueEntries;
        private List<Epos4EnumEntry> _enumEntries;

        public AllowedValues(DataTypeList dataTypeList)
        {
            _dataTypeList = dataTypeList;
        }

        public string UniqueId { get; set; }

        public Range Range { get; private set; }

        public List<Epos4ValueEntry> ValueEntries => _valueEntries ?? (_valueEntries = new List<Epos4ValueEntry>());

        public List<Epos4EnumEntry> EnumEntries => _enumEntries ?? (_enumEntries = new List<Epos4EnumEntry>());
        
        public void LimitRange<T>(ref T minValue, ref T maxValue)
        {
            Range?.Limit(ref minValue, ref maxValue);
        }
        
        public bool Parse(XmlNode node)
        {
            bool result = true;

            try
            {
                var uniqueIdAttribute = node.Attributes?["uniqueID"];

                if (uniqueIdAttribute != null)
                {
                    UniqueId = uniqueIdAttribute.InnerXml;
                }

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "range")
                    {
                        var range = new Range();

                        if (!range.Parse(childNode))
                        {
                            result = false;
                            break;
                        }

                        Range = range;
                    }
                    else if (childNode.Name == "enumEntryIDRef")
                    {
                        var uniqueIdRefAttribute = childNode.Attributes?["uniqueIDRef"];

                        if (uniqueIdRefAttribute != null)
                        {
                            var uniqueIdRef = uniqueIdRefAttribute.InnerXml;
                            var enumEntry = _dataTypeList.FindReferenceEnumEntry(uniqueIdRef);

                            if (enumEntry == null)
                            {
                                result = false;
                                break;
                            }

                            EnumEntries.Add(enumEntry);
                        }
                    }
                    else if (childNode.Name == "valueEntry")
                    {
                        var valueEntry = new Epos4ValueEntry();

                        if (!valueEntry.Parse(childNode))
                        {
                            result = false;
                            break;
                        }

                        ValueEntries.Add(valueEntry);
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine(e);
            }

            return result;
        }

        public void GetEnumRange(ref List<long> enumRange)
        {
            foreach (var enumEntry in EnumEntries)
            {
                enumRange.Add(enumEntry.Value);
            }
        }

        public void GetEnumRange(ref List<string> enumRange)
        {
            foreach (var enumEntry in EnumEntries)
            {
                enumRange.Add(enumEntry.GetLabel(RegionalOptions.Language));
            }
        }

        public void GetValueRange(ref List<long> valueRange)
        {
            foreach (var valueEntry in ValueEntries)
            {
                valueRange.Add(valueEntry.Value.ToLong());
            }
        }
    }
}
