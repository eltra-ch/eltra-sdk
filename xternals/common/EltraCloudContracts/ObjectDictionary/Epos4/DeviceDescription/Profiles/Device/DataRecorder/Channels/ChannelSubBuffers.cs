using System.Collections.Generic;
using System.Xml;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraCloudContracts.ObjectDictionary.Epos4.DeviceDescription.Profiles.Device.DataRecorder.Channels
{
    public class ChannelSubBuffers
    {
        #region Properties

        public List<XddParamIdRef> ParamIdRefs { get; set; }
        public List<Parameter> Parameters { get; set; }

        #endregion

        #region Methods

        public bool Parse(XmlNode node)
        {
            bool result = true;

            ParamIdRefs = new List<XddParamIdRef>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "paramIDRef")
                {
                    var paramIdRef = new XddParamIdRef();

                    if (!paramIdRef.Parse(childNode))
                    {
                        result = false;
                        break;
                    }

                    ParamIdRefs.Add(paramIdRef);
                }
            }

            return result;
        }

        public void Resolve(XddParameterList parameterList)
        {
            Parameters = new List<Parameter>();

            foreach (var paramIdRef in ParamIdRefs)
            {
                if (parameterList.FindParameter(paramIdRef.UniqueIdRef) is Parameter parameter)
                {
                    Parameters.Add(parameter);
                }
            }
        }

        #endregion


    }
}
