using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace EltraConnector.Master.Device.SerialNumber
{
    /// <inheritdoc/>
    public class SerialNumberProvider : ISerialNumberProvider
    {
        /// <inheritdoc/>
        public ulong ReadSerialNumber()
        {
            ulong result = 0x1000;

            var nifs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nif in nifs)
            {
                if(nif.OperationalStatus == OperationalStatus.Up)
                {
                    var mac = nif.GetPhysicalAddress();
                    
                    result = (ulong)mac.GetHashCode();

                    break;
                }
            }

            return result;
        }
    }
}
