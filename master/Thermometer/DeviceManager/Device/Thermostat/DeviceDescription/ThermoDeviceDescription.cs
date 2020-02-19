using System;
using System.Threading.Tasks;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;

namespace ThermoMaster.DeviceManager.Device.Thermostat.ObjectDictionary
{
    public class ThermoDeviceDescription : DeviceDescriptionFile
    {
        public ThermoDeviceDescription(EltraDevice device) : 
            base(device)
        {   
        }

        public override async Task Read()
        {
            Content = await GetContentFromResources();

            ReadProductName();

            OnDeviceDescriptionStateChanged(new DeviceDescriptionEventArgs { DeviceDescription = this, State = DeviceDescriptionState.Read });
        }

        protected override void ReadProductName()
        {
            try
            {
                if (!string.IsNullOrEmpty(Content))
                {
                    var doc = new XmlDocument();

                    doc.LoadXml(Content);

                    var rootNode = doc.DocumentElement;

                    var productNameNode = rootNode?.SelectSingleNode("Profile/ProfileBody/DeviceIdentity/productName");

                    if (productNameNode != null) ProductName = productNameNode.InnerText;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadProductName", e);
            }
        }
    }
}
