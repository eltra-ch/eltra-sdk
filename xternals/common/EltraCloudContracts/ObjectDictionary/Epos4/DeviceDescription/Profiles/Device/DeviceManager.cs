using System.Xml;

using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.UserLevels;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device
{
    public class DeviceManager
    {
        #region Private fields

        private UserLevelList _userLevelList;

        private DataRecorderList _dataRecorderList;

        private UserLevelList UserLevelList => _userLevelList ?? (_userLevelList = new UserLevelList(this));

        public DataRecorderList DataRecorderList => _dataRecorderList ?? (_dataRecorderList = new DataRecorderList(this));

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
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
                else if (childNode.Name == "dataRecorderList")
                {
                    if (!DataRecorderList.Parse(childNode))
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        public UserLevel FindReference(string uniqueId)
        {
            UserLevel result = null;

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
        public void ResolveParameterReferences(ParameterList parameterList)
        {
            DataRecorderList.ResolveParameterReferences(parameterList);
        }

        #endregion
    }
}
