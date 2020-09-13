using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetPositionMarkerParameterCommand : DeviceCommand
	{
		public SetPositionMarkerParameterCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetPositionMarkerParameter";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
