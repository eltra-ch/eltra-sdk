using System;
using System.IO;
using System.Xml;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription
{
    public class XddDeviceDescription : Dd
    {
        #region Private fields

        private XddProfile _profile;
        private readonly EltraDevice _device;

        #endregion

        #region Constructors

        public XddDeviceDescription(EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        protected XddProfile Profile
        {
            get => _profile ?? (_profile = new XddProfile(_device));
            set => _profile = value;
        } 
        
        #endregion

        #region Methods

        public override bool Parse()
        {
            bool result = false;

            try
            {
                var doc = new XmlDocument();

                if (File.Exists(DataSource))
                {
                    doc.Load(DataSource);
                    result = true;
                }
                else if(!string.IsNullOrEmpty(DataSource))
                {
                    doc.LoadXml(DataSource);
                    result = true;
                }

                if(result)
                {
                    var rootNode = doc.DocumentElement;

                    foreach (XmlNode childNode in rootNode.ChildNodes)
                    {
                        if (childNode.Name == "Profile")
                        {
                            result = Profile.Parse(childNode);
                            break;
                        }
                    }

                    if (result)
                    {
                        Parameters = Profile.ParameterList.Parameters;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            
            return result;
        }

        #endregion
    }
}
