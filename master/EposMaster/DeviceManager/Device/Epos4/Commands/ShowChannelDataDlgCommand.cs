using System.Runtime.Serialization;
using EltraCloudContracts.Contracts.CommandSets;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ShowChannelDataDlgCommand : DeviceCommand
	{
		public ShowChannelDataDlgCommand(EltraCloudContracts.Contracts.Devices.EltraDevice device)
			:base(device)
		{
			Name = "ShowChannelDataDlg";
		}

		public override bool Execute(string source)
		{
			//TODO
			return true;
		}
	}
}
