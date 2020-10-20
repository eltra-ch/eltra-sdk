using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDeviceNameCommand : DeviceCommand
	{
		public GetDeviceNameCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetDeviceName";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
