using System;
using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters
{
    public class XddAllowedValues
    {
        private readonly XddDataTypeList _dataTypeList;
        private List<XddValueEntry> _valueEntries;
        private List<XddEnumEntry> _enumEntries;

        public XddAllowedValues(XddDataTypeList dataTypeList)
        {
            _dataTypeList = dataTypeList;
        }

        public string UniqueId { get; set; }

        public XddRange Range { get; private set; }

        public List<XddValueEntry> ValueEntries => _valueEntries ?? (_valueEntries = new List<XddValueEntry>());

        public List<XddEnumEntry> EnumEntries => _enumEntries ?? (_enumEntries = new List<XddEnumEntry>());
        
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
                        var range = new XddRange();

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
                        var valueEntry = new XddValueEntry();

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
