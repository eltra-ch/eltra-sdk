using EltraCloudContracts.Contracts.Devices;
using System;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Identity
{
    public class XddDeviceVersion
    {
        private EltraDevice _device;

        public XddDeviceVersion(EltraDevice device)
        {
            _device = device;
        }
                
        public ushort HardwareVersion { get; set; }
        
        public ushort SoftwareVersion { get; set; }
        
        public ushort ApplicationNumber { get; set; }
        
        public ushort ApplicationVersion { get; set; }

        public bool Parse(XmlNode childNode)
        {
            bool result = false;

            if (childNode != null)
            {
                var versionTypeAttribute = childNode.Attributes["versionType"];
                
                if (versionTypeAttribute != null)
                {
                    switch(versionTypeAttribute.InnerText)
                    {
                        case "SW":
                            SoftwareVersion = Convert.ToUInt16(childNode.InnerXml, 16);
                            break;
                        case "HW":
                            HardwareVersion = Convert.ToUInt16(childNode.InnerXml, 16);
                            break;
                        case "APPNB":
                            ApplicationNumber = Convert.ToUInt16(childNode.InnerXml, 16);
                            break;
                        case "APPVER":
                            ApplicationVersion = Convert.ToUInt16(childNode.InnerXml, 16);
                            break;
                    }

                    result = true;
                }
            }

            return result;
        }

        public bool UpdateVersion()
        {
            bool result = false;

            if (_device != null)
            {
                var version = _device.Version;

                if (version != null)
                {
                    version.SoftwareVersion = SoftwareVersion;
                    version.HardwareVersion = HardwareVersion;
                    version.ApplicationNumber = ApplicationNumber;
                    version.ApplicationVersion = ApplicationVersion;

                    result = true;
                }
            }

            return result;
        }
    }
}
