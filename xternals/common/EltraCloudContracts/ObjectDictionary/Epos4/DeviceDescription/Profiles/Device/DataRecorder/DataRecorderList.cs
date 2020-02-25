﻿using System.Collections.Generic;
using System.Xml;

using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Device;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder
{
    public class DataRecorderList
    {
        #region Private fields
        
        private readonly XddDeviceManager _deviceManager;
        private List<DataRecorder> _dataRecorder;

        #endregion

        #region Constructors

        public DataRecorderList(XddDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        #endregion

        #region Properties

        public List<DataRecorder> DataRecorders
        {
            get => _dataRecorder ?? (_dataRecorder = new List<DataRecorder>());
        }
        
        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "dataRecorder")
                {
                    var dataRecorder = new DataRecorder();

                    if (!dataRecorder.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    DataRecorders.Add(dataRecorder);
                }
            }

            return result;
        }

        #endregion

        public void ResolveParameterReferences(XddParameterList parameterList)
        {
            foreach (var dataRecorder in DataRecorders)
            {
                dataRecorder.ResolveParameterReferences(parameterList);
            }
        }
    }
}
