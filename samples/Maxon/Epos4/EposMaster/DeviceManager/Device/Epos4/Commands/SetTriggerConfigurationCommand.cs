using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Converters;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class SetTriggerConfigurationCommand : DeviceCommand
	{
        public SetTriggerConfigurationCommand()
        { }
		
        public SetTriggerConfigurationCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetTriggerConfiguration";
            
            AddParameter("Index", TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);
            AddParameter("Mask", TypeCode.UInt32);
            AddParameter("Mode", TypeCode.Byte);
            AddParameter("HighValue", TypeCode.UInt32);
            AddParameter("LowValue", TypeCode.UInt32);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out SetTriggerConfigurationCommand result))
            {
                result.Device = Device;
            }

            return result;
        }

		public override bool Execute(string source)
		{
            bool result = false;
            var eposDevice = Device as Epos4Device;
            var communication = eposDevice?.Communication;
            bool commandResult = false;
            
            if (communication != null)
            {
                ushort index = 0;
                byte subIndex = 0;
                byte mode = 0;
                uint mask = 0;
                uint highValue = 0;
                uint lowValue = 0;
                
                GetParameterValue("Index", ref index);
                GetParameterValue("SubIndex", ref subIndex);
                GetParameterValue("Mask", ref mask);
                GetParameterValue("Mode", ref mode);
                GetParameterValue("HighValue", ref highValue);
                GetParameterValue("LowValue", ref lowValue);

                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary objectDictionary)
                {
                    var triggerModeParameter = objectDictionary.GetRecorderTriggerModeParameter();
                    var triggerMaskParameter = objectDictionary.GetRecorderTriggerMaskParameter();
                    var triggerHighValueParameter = objectDictionary.GetRecorderTriggerHighValueParameter();
                    var triggerLowValueParameter = objectDictionary.GetRecorderTriggerLowValueParameter();
                    var triggerVariableParameter = objectDictionary.GetRecorderTriggerVariableParameter();

                    commandResult = eposDevice.WriteParameterValue(triggerModeParameter, mode);

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.WriteParameterValue(triggerHighValueParameter, highValue);
                    }
                    
                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.WriteParameterValue(triggerLowValueParameter, lowValue);
                    }

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.WriteParameterValue(triggerMaskParameter, mask);
                    }

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.WriteParameterValue(triggerVariableParameter, ObjectDictionaryConverter.ConvertAddressToVariable(index, subIndex));
                    }
                }
                
                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
