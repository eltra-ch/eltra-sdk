using EltraCloudContracts.Contracts.Devices;
using EltraCommon.Helpers;
using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    [DataContract]
    public class DeviceDescription
    {
        private string _plainContent;

        public DeviceDescription()
        {
        }

        public DeviceDescription(EltraDevice device)
        {
            Init(device);
        }

        [DataMember]
        public ulong SerialNumber { get; set; }

        [DataMember]
        public DeviceVersion Version { get; set; }

        [DataMember]
        public string Content { get; set; }

        public string PlainContent
        {
            get
            {
                if (!string.IsNullOrEmpty(Content))
                {
                    var data = Convert.FromBase64String(Content);

                    _plainContent = ZipHelper.Deflate(data);
                }
                
                return _plainContent;
            }
            private set
            {
                _plainContent = value;
            }
        }

        [DataMember]
        public string Encoding { get; set; }

        public string HashCode 
        { 
            get
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(PlainContent))
                {
                    result = CryptHelpers.ToMD5(PlainContent);
                }

                return result;
            }
        }

        [DataMember]
        public DateTime Modified { get; set; }

        private void Init(EltraDevice device)
        {
            if (device != null)
            {                
                SerialNumber = device.Identification.SerialNumber;
                PlainContent = device.DeviceDescription?.Content;

                if (!string.IsNullOrEmpty(PlainContent))
                {
                    Content = Convert.ToBase64String(ZipHelper.Compress(PlainContent));
                    Encoding = "base64/zip";
                }

                Version = device.Version;
            }

            Modified = DateTime.Now;
        }

    }
}
