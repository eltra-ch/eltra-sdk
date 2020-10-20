using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Converters;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class GetTriggerConfigurationCommand : DeviceCommand
	{
        public GetTriggerConfigurationCommand()
        { }
		
        public GetTriggerConfigurationCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetTriggerConfiguration";
            
            AddParameter("Index", TypeCode.UInt16, ParameterType.Out);
            AddParameter("SubIndex", TypeCode.Byte, ParameterType.Out);
            AddParameter("Mask", TypeCode.UInt32, ParameterType.Out);
            AddParameter("Mode", TypeCode.Byte, ParameterType.Out);
            AddParameter("HighValue", TypeCode.UInt32, ParameterType.Out);
            AddParameter("LowValue", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out GetTriggerConfigurationCommand result))
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
                uint highValue = 0;
                uint lowValue = 0;
                uint mask = 0;

                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary objectDictionary)
                {
                    var triggerModeParameter = objectDictionary.GetRecorderTriggerModeParameter();
                    var triggerHighValueParameter = objectDictionary.GetRecorderTriggerHighValueParameter();
                    var triggerLowValueParameter = objectDictionary.GetRecorderTriggerLowValueParameter();
                    var triggerVariableParameter = objectDictionary.GetRecorderTriggerVariableParameter();
                    var triggerMaskParameter = objectDictionary.GetRecorderTriggerMaskParameter();

                    commandResult = eposDevice.ReadParameterValue(triggerModeParameter, out mode);

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.ReadParameterValue(triggerMaskParameter, out mask);
                    }

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.ReadParameterValue(triggerHighValueParameter, out highValue);
                    }

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.ReadParameterValue(triggerLowValueParameter, out lowValue);
                    }

                    if (commandResult)
                    {
                        commandResult =
                            eposDevice.ReadParameterValue(triggerVariableParameter, out uint triggerVariable);

                        ObjectDictionaryConverter.ConvertToAddress(BitConverter.GetBytes(triggerVariable), ref index, ref subIndex);
                    }
                }

                SetParameterValue("Index", index);
                SetParameterValue("SubIndex", subIndex);
                SetParameterValue("Mask", mask);
                SetParameterValue("Mode", mode);
                SetParameterValue("HighValue", highValue);
                SetParameterValue("LowValue", lowValue);

                SetParameterValue("ErrorCode", communication.LastErrorCode);
                SetParameterValue("Result", commandResult);

                result = true;
            }

            return result;
        }
	}
}
