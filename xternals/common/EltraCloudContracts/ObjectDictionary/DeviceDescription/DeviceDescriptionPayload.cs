using EltraCloudContracts.Contracts.Devices;
using EltraCommon.Helpers;
using System;
using System.Runtime.Serialization;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    [DataContract]
    public class DeviceDescriptionPayload
    {
        #region Private fields

        private string _plainContent;

        #endregion

        #region Constructors

        public DeviceDescriptionPayload()
        {
        }

        public DeviceDescriptionPayload(EltraDevice device)
        {
            Init(device);
        }

        #endregion

        #region Properties

        [DataMember]
        public string CallerUuid { get; set; }

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

        #endregion

        #region Methods

        private void Init(EltraDevice device)
        {
            if (device != null)
            {                
                SerialNumber = device.Identification.SerialNumber;
                PlainContent = device.DeviceDescription?.DataSource;

                if (!string.IsNullOrEmpty(PlainContent))
                {
                    Content = Convert.ToBase64String(ZipHelper.Compress(PlainContent));
                    Encoding = "base64/zip";
                }

                Version = device.Version;
            }

            Modified = DateTime.Now;
        }

        #endregion
    }
}
