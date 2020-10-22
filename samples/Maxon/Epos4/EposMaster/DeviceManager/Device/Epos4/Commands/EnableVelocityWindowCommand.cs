using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnableVelocityWindowCommand : DeviceCommand
	{
		public EnableVelocityWindowCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnableVelocityWindow";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
