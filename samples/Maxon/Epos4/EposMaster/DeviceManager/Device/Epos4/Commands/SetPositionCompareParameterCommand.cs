using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionCompareParameterCommand : DeviceCommand
	{
		public SetPositionCompareParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionCompareParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
