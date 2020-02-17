/* Copyright (c) Dawid Sienkiewicz - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Dawid Sienkiewicz <dsienkiewicz@outlook.com>, February 2018
 */

using EltraCloudContracts.Contracts.Sessions;

#pragma warning disable CS1591

namespace EltraCloud.Ip2Location
{
    public class Db5Entry : IpLocation
    {
        public long From { get; set; }
        public long To { get; set; }

        public void CopyTo(ref IpLocation location)
        {
            location.CountryCode = CountryCode;
            location.Country = Country;
            location.Region = Region;
            location.City = City;
            location.Latitude = Latitude;
            location.Longitude = Longitude;
        }
    }
}
