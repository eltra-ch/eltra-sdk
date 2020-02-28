using EltraCloudContracts.Contracts.Devices;
using System.Xml;

namespace EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Identity
{
    public class XddDeviceIdentity
    {
        private EltraDevice _device;

        private XddDeviceFamily _deviceFamily;
        private XddProductText _productText;
        private XddProductUrl _productUrl;
        private XddDeviceVersion _deviceVersion;

        public XddDeviceIdentity(EltraDevice device)
        {
            _device = device;
        }

        public XddDeviceFamily DeviceFamily => _deviceFamily ?? (_deviceFamily = new XddDeviceFamily());
        public XddProductText ProductText => _productText ?? (_productText = new XddProductText());
        public XddProductUrl ProductUrl => _productUrl ?? (_productUrl = new XddProductUrl());

        public XddDeviceVersion DeviceVersion => _deviceVersion ?? (_deviceVersion = new XddDeviceVersion(_device));

        public string VendorName { get; set; }
        public string ProductFamily { get; set; }
        public string ProductName { get; set; }
        public string ProductPicture { get; set; }
        public string OrderNumber { get; set; }

        public bool Parse(XmlNode profileBodyNode)
        {
            bool result = false;

            if (profileBodyNode != null)
            {
                foreach (XmlNode childNode in profileBodyNode.ChildNodes)
                {
                    if (childNode.Name == "vendorName")
                    {
                        VendorName = childNode.InnerXml;
                    }
                    else if(childNode.Name == "deviceFamily")
                    {
                        if(!DeviceFamily.Parse(childNode))
                        {
                            result = false;
                            break;
                        }
                    }
                    else if(childNode.Name == "productFamily")
                    {
                        ProductFamily = childNode.InnerXml;
                    }
                    else if (childNode.Name == "productName")
                    {
                        ProductName = childNode.InnerXml;
                        result = true;
                    }
                    else if (childNode.Name == "productText")
                    {
                        if (!ProductText.Parse(childNode))
                        {
                            result = false;
                            break;
                        }
                    }
                    else if (childNode.Name == "productUrl")
                    {
                        if (!ProductUrl.Parse(childNode))
                        {
                            result = false;
                            break;
                        }
                    }
                    else if (childNode.Name == "productPicture")
                    {
                        ProductPicture = childNode.InnerXml;
                    }
                    else if (childNode.Name == "orderNumber")
                    {
                        OrderNumber = childNode.InnerXml;
                    }
                    else if (childNode.Name == "version")
                    {
                        if (!DeviceVersion.Parse(childNode))
                        {
                            result = false;
                            break;
                        }
                    }
                }

                if(result)
                {
                    UpdateDeviceVersion();
                    UpdateProductName();
                    UpdateProductPicture();
                }
            }

            return result;
        }

        private void UpdateDeviceVersion()
        {
            DeviceVersion.UpdateVersion();
        }

        private void UpdateProductName()
        {
            if (_device != null)
            {
                _device.Name = ProductName;
            }
        }

        private void UpdateProductPicture()
        {
            if (_device != null)
            {
                _device.ProductPicture = ProductPicture;
            }
        }
    }
}
