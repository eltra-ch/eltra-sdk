using System;
using System.Xml;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    public class XddDeviceDescriptionFile : DeviceDescriptionFile
    {
        public XddDeviceDescriptionFile(EltraDevice device) 
            : base(device)
        {            
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

        protected override bool ReadDeviceVersion()
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(Content))
                {
                    var doc = new XmlDocument();

                    doc.LoadXml(Content);

                    var rootNode = doc.DocumentElement;

                    var versionNodes = rootNode?.SelectNodes("Profile/ProfileBody/DeviceIdentity/version");

                    if (versionNodes != null)
                    {
                        var version = new DeviceVersion();

                        foreach (XmlNode versionNode in versionNodes)
                        {
                            var versionTypeAttribute = versionNode.Attributes["versionType"];

                            if(versionTypeAttribute!=null)
                            {
                                switch (versionTypeAttribute.InnerText)
                                {
                                    case "SW": version.SoftwareVersion = Convert.ToUInt16(versionNode.InnerText, 16); break;
                                    case "HW": version.HardwareVersion = Convert.ToUInt16(versionNode.InnerText, 16); break;
                                    case "APPNB": version.ApplicationNumber = Convert.ToUInt16(versionNode.InnerText, 16); break;
                                    case "APPVER": version.ApplicationVersion = Convert.ToUInt16(versionNode.InnerText, 16); break;
                                }
                            }
                        }

                        Device.Version = version;

                        result = true;
                    }   
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadDeviceVersion", e);
            }

            return result;
        }
    }
}
