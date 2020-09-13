using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetVersionCommand : DeviceCommand
	{
		public GetVersionCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetVersion";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
