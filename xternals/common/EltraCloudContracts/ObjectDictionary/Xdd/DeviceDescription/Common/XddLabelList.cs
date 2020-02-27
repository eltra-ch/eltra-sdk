using System.Collections.Generic;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Common
{
    public class XddLabelList
    {
        private List<XddLabel> _labels;

        public List<XddLabel> Labels => _labels ?? (_labels = new List<XddLabel>());

        public virtual bool Parse(XmlNode profileBodyNode)
        {
            bool result = false;

            if (profileBodyNode != null)
            {
                foreach (XmlNode childNode in profileBodyNode.ChildNodes)
                {
                    if (childNode.Name == "label")
                    {
                        var label = new XddLabel(childNode);

                        if (label.Parse())
                        {
                            Labels.Add(label);

                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}
