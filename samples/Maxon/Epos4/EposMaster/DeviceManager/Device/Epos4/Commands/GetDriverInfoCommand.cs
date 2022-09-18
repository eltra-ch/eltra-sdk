using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDriverInfoCommand : DeviceCommand
	{
		public GetDriverInfoCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetDriverInfo";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}
