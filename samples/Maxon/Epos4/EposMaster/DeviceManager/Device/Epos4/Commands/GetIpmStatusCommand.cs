using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetIpmStatusCommand : DeviceCommand
	{
		public GetIpmStatusCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetIpmStatus";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
