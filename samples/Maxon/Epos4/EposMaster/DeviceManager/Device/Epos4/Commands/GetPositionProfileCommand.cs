using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionProfileCommand : DeviceCommand
	{
		public GetPositionProfileCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetPositionProfile";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
