using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetTargetVelocityCommand : DeviceCommand
	{
		public GetTargetVelocityCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetTargetVelocity";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}
