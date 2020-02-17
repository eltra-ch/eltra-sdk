using System.Net;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCloud.Ip2Location;
using EltraCloudContracts.Contracts.Sessions;

#pragma warning disable CS1591

namespace EltraCloud.Services
{
    public class Ip2LocationService : IIp2LocationService
    {
        private readonly Db5Parser _db5Parser;

        public Ip2LocationService()
        {
            _db5Parser = new Db5Parser();
        }

        public string Ip2LocationFile { get; set; }
        
        public void Start()
        {
            Task.Run(
                () =>
                {
                    if (_db5Parser.Load(Ip2LocationFile))
                    {
                        MsgLogger.WriteLine($"Ip2Location file '{Ip2LocationFile}' successfully loaded");
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Start", $"Ip2Location file '{Ip2LocationFile}' load failed!");
                    }
                });
        }

        public override IpLocation FindAddress(IPAddress address)
        {
            IpLocation result = null;
            var db5Entry = _db5Parser.FindAddress(address);

            if (db5Entry != null)
            {
                result = new IpLocation(address.ToString());

                db5Entry.CopyTo(ref result);
            }

            return result;
        }
    }
}
