using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetMovementStateCommand : DeviceCommand
	{
		public GetMovementStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetMovementState";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
