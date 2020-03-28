using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Transport;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace EltraNotKauf.Endpoints
{
    public class ContactEndpoint
    {
        private readonly CloudTransporter _transporter;

        public ContactEndpoint()
        {
            _transporter = new CloudTransporter();

            CountryCode = "CH";
            LanguageCode = "de";
        }

        public ContactEndpoint(CloudTransporter transporter)
        {
            _transporter = transporter;

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

        public async Task<EltraCloudContracts.Enka.Contacts.Contact> GetContact()
        {
            EltraCloudContracts.Enka.Contacts.Contact result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var url = UrlHelper.BuildUrl(Url, "api/contacts/get", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<EltraCloudContracts.Enka.Contacts.Contact>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetContact", e);
            }

            return result;
        }

        public async Task<bool> SetContact(EltraCloudContracts.Enka.Contacts.Contact contact)
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var json = JsonConvert.SerializeObject(contact);

                var response = await _transporter.Post(Url, "api/contacts/set", json);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }
    }
}
