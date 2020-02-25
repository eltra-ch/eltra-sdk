﻿using System;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Trigger
{
    public class TriggerCondition
    {
        public XddParamIdRef ParamIdRef { get; set; }
        
        public Parameter Parameter { get; set; }
        public int DefaultValue { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "paramIDRef")
                {
                    var paramIdRef = new XddParamIdRef();

                    if (!paramIdRef.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ParamIdRef = paramIdRef;
                }
                else if (childNode.Name == "defaultValue")
                {
                    string defaultValue = childNode.InnerXml;

                    if (defaultValue.StartsWith("0x"))
                    {
                        DefaultValue = Convert.ToInt32(defaultValue.Substring(2), 16);
                    }
                }
            }

            return result;
        }

        public void Resolve(XddParameterList parameterList)
        {
            Parameter = parameterList.FindParameter(ParamIdRef.UniqueIdRef) as Parameter;
        }
    }
}
