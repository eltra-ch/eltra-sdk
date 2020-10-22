using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class EnablePositionWindowCommand : DeviceCommand
	{
		public EnablePositionWindowCommand(EltraDevice device)
			:base(device)
		{
			Name = "EnablePositionWindow";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
