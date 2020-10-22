using System.Runtime.Serialization;
using EltraCommon.Contracts.CommandSets;
using EltraCommon.Contracts.Devices;

namespace EposMaster.DeviceManager.Device.Epos4.Commands
{
    [DataContract]
	class ShowChannelDataDlgCommand : DeviceCommand
	{
		public ShowChannelDataDlgCommand(EltraDevice device)
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
