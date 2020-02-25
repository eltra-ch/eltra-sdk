using System.Collections.Generic;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device.UserLevels
{
    public class XddUserLevelList
    {
        #region Private fields

        private List<XddUserLevel> _userLevels;
        private readonly XddDeviceManager _deviceManager;

        #endregion

        #region Constructors

        public XddUserLevelList(XddDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        #endregion

        #region Properties
        
        public List<XddUserLevel> UserLevels => _userLevels ?? (_userLevels = new List<XddUserLevel>());

        public bool IsRoleUndefined => UserLevels.Count == 0;
        
        #endregion

        #region Methods

        public bool HasRole(string role)
        {
            bool result = false;

            foreach (var userLevel in UserLevels)
            {
                if (userLevel.Role == role)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "userLevelIDRef")
                {
                    if (childNode.Attributes != null)
                    {
                        var idRefAtt = childNode.Attributes["uniqueIDRef"];
                        XddUserLevel userLevel = null;

                        if (idRefAtt != null)
                        {
                            userLevel = _deviceManager.FindReference(idRefAtt.InnerXml);
                        }

                        if (userLevel == null)
                        {
                            result = false;
                            break;
                        }

                        UserLevels.Add(userLevel);
                    }
                }
                else if (childNode.Name == "userLevel")
                {
                    var userLevel = new XddUserLevel();

                    if (!userLevel.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    UserLevels.Add(userLevel);
                }
            }

            return result;  
        }

        #endregion

        
    }
}
