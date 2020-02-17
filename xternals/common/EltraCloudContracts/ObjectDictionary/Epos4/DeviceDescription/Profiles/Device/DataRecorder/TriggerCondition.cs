using System.Xml;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Trigger;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class TriggerCondition
    {
        public TriggerConditionParam TriggerConditionParam { get; set; }
        public TriggerConditionMode TriggerConditionMode { get; set; }
        public TriggerConditionHigh TriggerConditionHigh { get; set; }
        public TriggerConditionLow TriggerConditionLow { get; set; }
        public TriggerConditionMask TriggerConditionMask { get; set; }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "triggerConditionParam")
                {
                    var triggerConditionParam = new TriggerConditionParam();

                    if (!triggerConditionParam.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerConditionParam = triggerConditionParam;
                }
                else if (childNode.Name == "triggerConditionMode")
                {
                    var triggerConditionMode = new TriggerConditionMode();

                    if (!triggerConditionMode.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerConditionMode = triggerConditionMode;
                }
                else if (childNode.Name == "triggerConditionHigh")
                {
                    var triggerConditionHigh = new TriggerConditionHigh();

                    if (!triggerConditionHigh.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerConditionHigh = triggerConditionHigh;
                }
                else if (childNode.Name == "triggerConditionLow")
                {
                    var triggerConditionLow = new TriggerConditionLow();

                    if (!triggerConditionLow.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerConditionLow = triggerConditionLow;
                }
                else if (childNode.Name == "triggerConditionMask")
                {
                    var triggerConditionMask = new TriggerConditionMask();

                    if (!triggerConditionMask.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    TriggerConditionMask = triggerConditionMask;
                }
            }

            return result;
        }

        public void Resolve(ParameterList parameterList)
        {
            TriggerConditionParam.Resolve(parameterList);
            TriggerConditionMode.Resolve(parameterList);
            TriggerConditionLow.Resolve(parameterList);
            TriggerConditionHigh.Resolve(parameterList);
            TriggerConditionMask.Resolve(parameterList);
        }
    }
}
