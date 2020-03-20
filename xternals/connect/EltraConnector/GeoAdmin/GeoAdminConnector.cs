using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ConsoleApp5
{
    //{"results":
    //  [{"featureId": 36264139,
    //      "attributes": {"status": "g\u00fcltig",
    //                      "plzo": "6074 Giswil",
    //                      "gdename": "Giswil",
    //                      "official": 0,
    //                      "modified": "2019-09-12 01:37:58",
    //                      "label": "Dreiw\u00e4sserweg",
    //                      "gdenr": 1403,
    //                      "esid": 10071297,
    //                      "validated": 0,
    //                      "type":
    //                      "Strasse"},
    //      "layerBodId": "ch.swisstopo.amtliches-strassenverzeichnis",
    //      "layerName": "Official street index",
    //      "id": 36264139}]}

    class Rest
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task GetStreets(string streetPart)
        {
            string url =
                $"http://api3.geo.admin.ch/rest/services/api/MapServer/find?layer=ch.swisstopo.amtliches-strassenverzeichnis&searchText={HttpUtility.UrlEncode(streetPart)}&searchField=label&returnGeometry=false&contains=true";
            
            using (var response = await _client.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var json = await streamReader.ReadToEndAsync();

                            if (!string.IsNullOrEmpty(json))
                            {
                                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoStreetResults>(json);
                            }
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                }
            }
        }

        public async Task GetAddressCoordinates(string address)
        {
            string url =
                "https://api3.geo.admin.ch/rest/services/ech/SearchServer?lang=de&type=locations&limit=4&origins=address&searchText=" + HttpUtility.UrlEncode(address);

            using (var response = await _client.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var json = await streamReader.ReadToEndAsync();

                            if (!string.IsNullOrEmpty(json))
                            {
                                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoResults>(json);
                            }
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                }
            }
        }
    }
}
