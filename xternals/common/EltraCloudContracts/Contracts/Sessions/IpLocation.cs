namespace EltraCloudContracts.Contracts.Sessions
{
    public class IpLocation
    {
        public IpLocation()
        {
            Ip = "255.255.255.255";
        }
        public IpLocation(string ip)
        {
            Ip = ip;
        }

        public string Ip { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
