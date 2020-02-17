using System.Collections.Generic;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.UserLevels
{
    public class UserLevelList
    {
        #region Private fields

        private List<UserLevel> _userLevels;
        private readonly DeviceManager _deviceManager;

        #endregion

        #region Constructors

        public UserLevelList(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        #endregion

        #region Properties
        
        public List<UserLevel> UserLevels => _userLevels ?? (_userLevels = new List<UserLevel>());

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
                        UserLevel userLevel = null;

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
                    var userLevel = new UserLevel();

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
