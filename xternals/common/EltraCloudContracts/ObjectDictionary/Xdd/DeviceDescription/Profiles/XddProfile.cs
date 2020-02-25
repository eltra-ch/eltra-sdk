using System.Xml;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles
{
    public class XddProfile
    {
        #region Private fields

        private XddProfileBody _profileBody;
        private readonly Contracts.Devices.EltraDevice _device;

        #endregion

        #region Constructors

        public XddProfile(Contracts.Devices.EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        protected XddProfileBody ProfileBody
        {
            get => _profileBody ?? (_profileBody = new XddProfileBody(_device));
            set => _profileBody = value;
        }

        public XddParameterList ParameterList => ProfileBody.ParameterList;

        #endregion

        #region Methods

        public virtual bool Parse(XmlNode profileNode)
        {
            bool result = false;

            foreach (XmlNode childNode in profileNode.ChildNodes)
            {
                if (childNode.Name == "ProfileBody")
                {
                    result = ProfileBody.Parse(childNode);
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
