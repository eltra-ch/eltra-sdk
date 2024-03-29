using System;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.ObjectDictionary.Epos4;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class DeactivateAllChannelsCommand : DeviceCommand
	{
        public DeactivateAllChannelsCommand()
        { }
		
        public DeactivateAllChannelsCommand(EltraDevice device)
			:base(device)
		{
			Name = "DeactivateAllChannels";

            AddParameter("Result", TypeCode.Boolean, ParameterType.Out);
            AddParameter("ErrorCode", TypeCode.UInt32, ParameterType.Out);
        }

        public override DeviceCommand Clone()
        {
            if (Clone(out DeactivateAllChannelsCommand result))
            {
                result.Device = Device;
            }

            return result;
        }

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
            bool result = false;
            var eposDevice = Device as Epos4Device;
            var communication = eposDevice?.Communication;
            bool commandResult = false;

            if (communication != null)
            {
                if (eposDevice.ObjectDictionary is Epos4ObjectDictionary obd)
                {
                    byte channelCount = obd.GetRecorderChannelCount();

                    for (byte channelNumber = 0; channelNumber < channelCount; channelNumber++)
                    {
                        var channelParameter = obd.GetRecorderChannelParameter(channelNumber);

                        if (channelParameter != null)
                        {
                            commandResult = communication.SetObject(channelParameter.Index,
                                channelParameter.SubIndex, BitConverter.GetBytes(0));

                            if (!commandResult)
                            {
                                break;
                            }
                        }
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
