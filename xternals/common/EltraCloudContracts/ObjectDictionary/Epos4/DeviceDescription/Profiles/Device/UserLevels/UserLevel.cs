using System.Collections.Generic;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Common;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.UserLevels
{
    public class UserLevel
    {
        #region Private fields

        private List<Label> _labels;
        private List<Description> _descriptions;

        #endregion

        #region Properties
        
        public string UniqueId { get; set; }

        public List<Label> Labels => _labels ?? (_labels = new List<Label>());

        public List<Description> Descriptions => _descriptions ?? (_descriptions = new List<Description>());

        public string Role { get; set; }

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;
            var idAtt = node.Attributes["uniqueID"];

            if (idAtt != null)
            {
                UniqueId = idAtt.InnerXml;

                foreach (XmlNode childNode in node)
                {
                    if (childNode.Name == "label")
                    {
                        var label = new Epos4Label(childNode);

                        if (!label.Parse())
                        {
                            result = false;
                            break;
                        }

                        Labels.Add(label);
                    }
                    else if (childNode.Name == "description")
                    {
                        var description = new Epos4Description(childNode);

                        if (!description.Parse())
                        {
                            result = false;
                            break;
                        }

                        Descriptions.Add(description);
                    }
                    else if (childNode.Name == "role")
                    {
                        Role = childNode.InnerXml.Trim();
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
