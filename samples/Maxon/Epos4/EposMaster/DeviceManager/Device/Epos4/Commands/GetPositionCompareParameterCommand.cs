using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class GetPositionCompareParameterCommand : DeviceCommand
	{
		public GetPositionCompareParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "GetPositionCompareParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
