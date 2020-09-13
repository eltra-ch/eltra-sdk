using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetGatewaySettingsCommand : DeviceCommand
	{
		public GetGatewaySettingsCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetGatewaySettings";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
