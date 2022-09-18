using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetDeviceErrorCodeCommand : DeviceCommand
	{
		public GetDeviceErrorCodeCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetDeviceErrorCode";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}
