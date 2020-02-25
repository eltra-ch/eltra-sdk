using System;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.ObjectDictionary.Epos4;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    public class DeactivateAllChannelsCommand : DeviceCommand
	{
        public DeactivateAllChannelsCommand()
        { }
		
        public DeactivateAllChannelsCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
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

		public override bool Execute(string source)
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
