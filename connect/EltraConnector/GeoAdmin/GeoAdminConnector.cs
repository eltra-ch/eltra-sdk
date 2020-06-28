using EltraCloudContracts.GeoAdmin;
using EltraCommon.Logger;
using EltraConnector.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace EltraConnector.GeoAdmin
{
    public class GeoAdminConnector
    {
        private readonly CloudTransporter _transporter = new CloudTransporter();

        public GeoAdminConnector()
        {
            GeoAdminRestServicesUrl = "https://api3.geo.admin.ch/rest/services";
            GeoAdminMapServerUrl = $"{GeoAdminRestServicesUrl}/api/MapServer";
            GeoAdminSearchServerUrl = $"{GeoAdminRestServicesUrl}/ech/SearchServer";
        }

        public string GeoAdminRestServicesUrl { get; set; }
        protected string GeoAdminMapServerUrl { get; set; }
        protected string GeoAdminSearchServerUrl { get; set; }

        public async Task<List<GeoStreetInfo>> GetStreetsInfo(string streetPart)
        {
            var result = new List<GeoStreetInfo>();
            string url =
                $"{GeoAdminMapServerUrl}/find?layer=ch.swisstopo.amtliches-strassenverzeichnis&searchText={HttpUtility.UrlEncode(streetPart)}&searchField=label&returnGeometry=false&contains=true";

            try
            {

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var geoStreetResults = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoStreetResults>(json);

                    if(geoStreetResults!=null)
                    {
                        foreach(var streetResult in geoStreetResults.Results)
                        {
                            var streetInfo = new GeoStreetInfo();

                            streetInfo.City = streetResult.Attrs.Municipality;
                            streetInfo.Street = streetResult.Attrs.Label;

                            streetInfo.PostalCode = ExtractSwissPostalCode(streetResult.Attrs.PostalCode);

                            result.Add(streetInfo);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetStreets", e);
            }

            return result;
        }

        private static string ExtractSwissPostalCode(string geoPostalCode)
        {
            const int ChPostalCodeLength = 4;
            string result = string.Empty;

            for (int i = 0; i < geoPostalCode.Length; i++)
            {
                if (char.IsDigit(geoPostalCode[i]))
                {
                    result += geoPostalCode[i];

                    if (result.Length == ChPostalCodeLength)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// GetAddressCoordinates
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<List<GeoCoordinates>> GetAddressCoordinates(string address)
        {
            string url =
                $"{GeoAdminSearchServerUrl}?lang=de&type=locations&limit=4&origins=address&searchText=" + HttpUtility.UrlEncode(address);
            var result = new List<GeoCoordinates>();

            try
            {
                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var geoResults = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoResults>(json);

                    if (geoResults != null)
                    {
                        foreach (var geoResult in geoResults.Results)
                        {
                            var coordinates = new GeoCoordinates();

                            coordinates.Latitude = geoResult.Attrs.Latitude;
                            coordinates.Longitude = geoResult.Attrs.Longitude;

                            result.Add(coordinates);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetAddressCoordinates", e);
            }

            return result;
        }
    }
}
