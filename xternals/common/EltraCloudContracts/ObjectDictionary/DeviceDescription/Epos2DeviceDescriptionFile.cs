using System;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.Devices;

namespace EltraCloudContracts.ObjectDictionary.DeviceDescription
{
    class Epos2DeviceDescriptionFile : DeviceDescriptionFile
    {
        public Epos2DeviceDescriptionFile(EltraDevice device) : base(device)
        {   
        }

        protected override void ReadProductName()
        {
            try
            {
                if (!string.IsNullOrEmpty(Content))
                {
                    foreach (var line in Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        const string productNameTag = "ProductName=";
                        if (line.Contains(productNameTag))
                        {
                            ProductName = line.Substring(productNameTag.Length).TrimEnd();

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Epos2DeviceDescriptionFile - ReadProductName", e);
            }
        }
    }
}
