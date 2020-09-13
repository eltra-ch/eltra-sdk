using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class SetVelocityUnitsCommand : DeviceCommand
	{
		public SetVelocityUnitsCommand(EltraDevice device)
			:base(device)
		{
			Name = "SetVelocityUnits";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
