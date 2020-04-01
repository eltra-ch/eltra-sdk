using EltraCloudContracts.Enka.Regional;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.GeoAdmin;
using EltraConnector.Transport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace EltraNotKauf.Endpoints
{
    public class RegionEndpoint
    {
        private readonly CloudTransporter _transporter;

        public RegionEndpoint()
        {
            _transporter = new CloudTransporter();

            CountryCode = "CH";
            LanguageCode = "de";
        }


        public string Url
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("url"))
                {
                    result = Application.Current.Properties["url"] as string;
                }

                return result;
            }
        }

        public string CountryCode { get; set; }

        public string LanguageCode { get; set; }

        public async Task<List<string>> GetPostalCodes(string regionCode, string startsWith)
        {
            var result = new List<string>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("countryCode", CountryCode);
                query.Add("regionCode", regionCode);

                var url = UrlHelper.BuildUrl(Url, "/api/Regional/postal-codes", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var postalCodes = JsonConvert.DeserializeObject<List<string>>(json);

                    if (!string.IsNullOrEmpty(startsWith))
                    {
                        foreach (var postalCode in postalCodes)
                        {
                            if (postalCode.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.Add(postalCode);
                            }
                        }
                    }

                    if (result.Count == 0)
                    {
                        result = postalCodes;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetCities", e);
            }
            
            return result;
        }

        public async Task<PostalCodeInfo> GetPostalCodeInfo(string postalCode)
        {
            PostalCodeInfo result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("countryCode", CountryCode);
                query.Add("postalCode", postalCode);
                query.Add("langCode", LanguageCode);

                var url = UrlHelper.BuildUrl(Url, "/api/Regional/postal-code-info", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var postalCodeInfo = JsonConvert.DeserializeObject<PostalCodeInfo>(json);

                    if (postalCodeInfo != null)
                    {
                        result = postalCodeInfo;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetPostalCodeInfo", e);
            }
            
            return result;
        }

        public async Task<List<string>> GetCities(string regionCode, string searchText)
        {
            var result = new List<string>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("countryCode", CountryCode);
                query.Add("regionCode", regionCode);

                var url = UrlHelper.BuildUrl(Url, "/api/Regional/cities", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var cities = JsonConvert.DeserializeObject<List<string>>(json);

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        foreach (var city in cities)
                        {
                            if (city.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.Add(city);
                            }
                        }
                    }

                    if (result.Count == 0)
                    {
                        result = cities;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetCities", e);
            }

            return result;
        }

        public async Task<List<string>> GetStreets(string city, string postalCode, string searchText)
        {
            var result = new List<string>();
            var geoConnector = new GeoAdminConnector();

            var streetInfos = await geoConnector.GetStreetsInfo(searchText);

            if (streetInfos != null)
            {
                foreach (var streetInfo in streetInfos)
                {
                    if (streetInfo.City == city || streetInfo.PostalCode == postalCode)
                    {
                        result.Add(streetInfo.Street);
                    }
                }
            }

            return result;
        }

        public async Task<List<EltraCloudContracts.Enka.Regional.Region>> ReadRegions()
        {
            var result = new List<EltraCloudContracts.Enka.Regional.Region>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("countryCode", CountryCode);
                query.Add("langCode", LanguageCode);

                var url = UrlHelper.BuildUrl(Url, "api/regional/regions", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<List<EltraCloudContracts.Enka.Regional.Region>>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadRegions", e);
            }

            return result;
        }
    }
}
