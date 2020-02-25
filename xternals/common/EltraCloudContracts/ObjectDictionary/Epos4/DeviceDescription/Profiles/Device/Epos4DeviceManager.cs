using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device
{
    public class Epos4DeviceManager : XddDeviceManager
    {
        #region Private fields

        private DataRecorderList _dataRecorderList;

        public DataRecorderList DataRecorderList => _dataRecorderList ?? (_dataRecorderList = new DataRecorderList(this));

        #endregion

        #region Methods

        public override bool Parse(XmlNode node)
        {
            bool result = base.Parse(node);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "dataRecorderList")
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

        public override void ResolveParameterReferences(XddParameterList parameterList)
        {
            base.ResolveParameterReferences(parameterList);

            DataRecorderList.ResolveParameterReferences(parameterList);
        }

        #endregion
    }
}
