using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles;
using EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription
{
    public class Xdd : Dd
    {
        #region Private fields

        private Profile _profile;
        private readonly EltraDevice _device;

        #endregion

        #region Constructors

        public Xdd(EltraDevice device)
        {
            _device = device;
        }

        #endregion

        #region Properties

        private Profile Profile => _profile ?? (_profile = new Profile(_device));
        
        public List<DataRecorder> DataRecorders { get; set; }

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
                        DataRecorders = Profile.DataRecorderList.DataRecorders;
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
