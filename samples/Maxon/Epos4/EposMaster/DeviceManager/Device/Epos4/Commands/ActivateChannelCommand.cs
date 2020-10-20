using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;
using EposMaster.DeviceManager.Converters;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class ActivateChannelCommand : DeviceCommand
	{
        public ActivateChannelCommand()
        { }
		
        public ActivateChannelCommand(EltraDevice device)
			:base(device)
		{
			Name = "ActivateChannel";

            AddParameter("Channel", TypeCode.Byte);
            AddParameter("Index", TypeCode.UInt16);
            AddParameter("SubIndex", TypeCode.Byte);

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out ActivateChannelCommand result))
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
                byte channelNumber = 0;

                GetParameterValue("Channel", ref channelNumber);
                GetParameterValue("Index", ref index);
                GetParameterValue("SubIndex", ref subIndex);

                if(eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    var channelParameter = obd.GetRecorderChannelParameter(channelNumber);
                    
                    if (channelParameter != null)
                    {
                        commandResult = communication.SetObject(channelParameter.Index, 
                                                            channelParameter.SubIndex,
                                                            ObjectDictionaryConverter.ConvertAddressToByteArray(index, subIndex));
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
