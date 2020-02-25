using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Templates;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Templates
{
    public class XddFlags : Flags
    {
        private readonly XmlNode _source;

        public XddFlags(XmlNode source)
        {
            _source = source;
        }
        
        #region Methods

        public bool Parse()
        {
            bool result = false;

            UniqueId = _source.Attributes["uniqueID"].InnerText;
            Name = _source.Attributes["name"].InnerText;
            
            foreach (XmlNode childNode in _source.ChildNodes)
            {
                if (childNode.Name == "Access")
                {
                    switch (childNode.InnerXml)
                    {
                        case "ro":
                            Access = AccessMode.ReadOnly;
                            break;
                        case "rw":
                            Access = AccessMode.ReadWrite;
                            break;
                        case "wo":
                            Access = AccessMode.WriteOnly;
                            break;
                    }

                    //TODO access WriteRestrictions att

                    result = true;    
                }
                else if (childNode.Name == "PdoMapping")
                {
                    //TODO flags PdoMapping
                }
                else if (childNode.Name == "Backup")
                {
                }
                else if (childNode.Name == "Setting")
                {
                }
                else if (childNode.Name == "Fieldbus1")
                {
                }
                else if (childNode.Name == "Fieldbus2")
                {
                }
            }

            return result;
        }

        #endregion
    }
}
