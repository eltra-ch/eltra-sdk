using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetHomingStateCommand : DeviceCommand
	{
		public GetHomingStateCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetHomingState";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
