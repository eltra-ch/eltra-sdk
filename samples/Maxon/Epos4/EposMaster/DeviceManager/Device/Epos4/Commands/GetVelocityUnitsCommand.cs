using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVelocityUnitsCommand : DeviceCommand
	{
		public GetVelocityUnitsCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetVelocityUnits";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}
