using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device.UserLevels;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device
{
    public class XddDeviceManager
    {
        #region Private fields

        private XddUserLevelList _userLevelList;

        private XddUserLevelList UserLevelList => _userLevelList ?? (_userLevelList = new XddUserLevelList(this));

        #endregion

        #region Methods

        public virtual bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "userLevelList")
                {
                    if (!UserLevelList.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        public XddUserLevel FindReference(string uniqueId)
        {
            XddUserLevel result = null;

            foreach (var userLevel in UserLevelList.UserLevels)
            {
                if (userLevel.UniqueId == uniqueId)
                {
                    result = userLevel;
                    break;
                }
            }

            return result;
        }

        public virtual void ResolveParameterReferences(XddParameterList parameterList)
        {            
        }

        #endregion
    }
}
