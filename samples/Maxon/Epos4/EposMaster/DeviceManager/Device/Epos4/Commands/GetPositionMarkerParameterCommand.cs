using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionMarkerParameterCommand : DeviceCommand
	{
		public GetPositionMarkerParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetPositionMarkerParameter";
		}

		public override bool Execute(string sourceChannelId, string sourceLoginName)
		{
			//TODO
			return true;
		}
	}
}
