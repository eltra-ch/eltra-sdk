using System;
using System.Xml;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    class Epos4DeviceDescriptionFile : DeviceDescriptionFile
    {
        public Epos4DeviceDescriptionFile(EltraDevice device) : base(device)
        {
            AddUrl("https://eltra.ddns.net/eltra/resources/fw/");
            
            FileExtension = "xdd";
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
                MsgLogger.Exception("Epos4DeviceDescriptionFile - ReadProductName", e);
            }
        }
    }
}
