using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetTargetPositionCommand : DeviceCommand
	{
		public GetTargetPositionCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetTargetPosition";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
