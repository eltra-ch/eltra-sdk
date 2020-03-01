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

        public override bool Equals(object obj)
        {
            bool result = false;
            
            if(obj is IpLocation location)
            {
                result = true;

                if(location.Ip != Ip)
                {
                    result = false;
                }

                if (result && location.CountryCode != CountryCode)
                {
                    result = false;
                }

                if (result && location.Country != Country)
                {
                    result = false;
                }

                if (result && location.Region != Region)
                {
                    result = false;
                }

                if (result && location.City != City)
                {
                    result = false;
                }

                if (result && location.Latitude != Latitude)
                {
                    result = false;
                }

                if (result && location.Longitude != Longitude)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
