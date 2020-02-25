﻿using System;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.DataTypes;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.DataTypes
{
    public class XddEnumEntry : EnumEntry
    {
        private readonly XmlNode _source;

        public XddEnumEntry(XmlNode source)
        {
            _source = source;
        }

        public string UniqueId { get; set; }


        #region Methods

        public override bool Parse()
        {
            bool result = false;

            UniqueId = _source.Attributes["uniqueID"].InnerXml;

            foreach (XmlNode childNode in _source)
            {
                if (childNode.Name == "value")
                {
                    var valueAsString = childNode.InnerXml;

                    Value = valueAsString.StartsWith("0x") ? Convert.ToInt64(valueAsString.Substring(2), 16) : Convert.ToInt64(valueAsString);

                    result = true;
                }
                else if (childNode.Name == "label")
                {
                    var label = new XddLabel(childNode);

                    if (label.Parse())
                    {
                        Labels.Add(label);
                        result = true;
                    }
                }
            }

            return result;
        }

        #endregion

        public string GetLabel(string lang)
        {
            string result = string.Empty;

            foreach (var label in Labels)
            {
                if (label.Lang == lang)
                {
                    result = label.Content;
                    break;
                }
            }

            return result;
        }
    }
}
