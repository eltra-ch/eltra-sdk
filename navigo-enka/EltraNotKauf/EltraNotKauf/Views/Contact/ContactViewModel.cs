using EltraNotKauf.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Web;
using EltraCommon.Helpers;
using EltraConnector.Transport;
using System;
using EltraCommon.Logger;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Collections.Generic;
using Region = EltraCloudContracts.Enka.Regional.Region;
using EltraCloudContracts.Enka.Regional;
using EltraConnector.GeoAdmin;

namespace EltraNotKauf.Views.Contact
{
    public class ContactViewModel : ToolViewModel
    {
        #region Private fields

        private bool _isValid;
        private CloudTransporter _transporter;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;

        private string _name;
        private List<Region> _regions;
        private string _phone;
        private string _street;
        private Region _region;
        private string _city;
        private List<string> _citySuggestions;
        private string _postalCode;
        private List<string> _postalCodeSuggestions;
        private List<string> _streetSuggestions;
        private string _notice;

        private string _countryCode = "CH";
        private string _langCode = "de";

        private bool _isPhoneValid;

        #endregion

        #region Constructors

        public ContactViewModel()
        {
            Title = "Addresse";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.home.png");
            IsMandatory = true;
            Uuid = "791AFBD3-E61D-4A0B-B35B-874D5A038E35";
            IsPhoneValid = true;

            _transporter = new CloudTransporter();

            PropertyChanged += (sender, args) => 
            { 
                if(args.PropertyName == "PostalCode")
                {
                    OnPostalCodeChanged();
                }
                if (args.PropertyName == "Phone")
                {
                    OnPhoneChanged();
                }

                UpdateValidFlag(); 
            }; 
        }

        #endregion

        #region Properties

        public bool IsPhoneValid
        {
            get => _isPhoneValid;
            set => SetProperty(ref _isPhoneValid, value);
        }

        public EltraCloudContracts.Enka.Contacts.Contact Contact
        {
            get => _contact ?? (_contact = new EltraCloudContracts.Enka.Contacts.Contact());
            set => _contact = value;
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
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

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Region Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
        
        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }
        
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public List<string> CitySuggestions
        {
            get => _citySuggestions;
            set => SetProperty(ref _citySuggestions, value);
        }

        public string PostalCode
        {
            get => _postalCode;
            set => SetProperty(ref _postalCode, value);
        }
        public List<string> PostalCodeSuggestions
        {
            get => _postalCodeSuggestions;
            set => SetProperty(ref _postalCodeSuggestions, value);
        }

        public List<string> StreetSuggestions
        {
            get => _streetSuggestions;
            set => SetProperty(ref _streetSuggestions, value);
        }

        public string Notice
        {
            get => _notice;
            set => SetProperty(ref _notice, value);
        }

        public List<Region> Regions
        {
            get => _regions ?? (_regions = new List<Region>());
            set => SetProperty(ref _regions, value);
        }

        #endregion

        #region Command

        public ICommand SaveCommand => new Command(OnSaveCommandClicked);
        public ICommand LocateCommand => new Command(OnLocateClicked);

        #endregion

        #region Methods

        public override void Clear()
        {
            Name = string.Empty;
            Phone = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            Notice = string.Empty;
        }

        private async void OnSaveCommandClicked(object obj)
        {
            await StoreContact();
        }

        private void UpdateValidFlag()
        {
            IsValid = !string.IsNullOrEmpty(Name) && IsPhoneValid;            
        }

        public override async void Show()
        {
            Regions = await ReadRegions();

            await ReadContact();

            UpdateValidFlag();

            base.Show();
        }

        private Region FindRegion(string name)
        {
            Region result = null;

            foreach (var region in Regions)
            {
                if (region.Name == name)
                {
                    result = region;
                    break;
                }
            }
            
            return result;
        }
        
        private async Task ReadContact()
        {
            Contact = await GetContact();

            if (Contact != null)
            {
                Name = Contact.Name;
                Phone = Contact.Phone;
                Street = Contact.Street;
                
                Region = FindRegion(Contact.Region);

                City = Contact.City;
                PostalCode = Contact.PostalCode;
                Notice = Contact.Notice;
            }
        }

        public async Task<List<string>> GetPostalCodes(string startsWith)
        {
            var result = new List<string>();

            if (Region != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query.Add("countryCode", _countryCode);
                    query.Add("regionCode", Region.ShortName);

                    var url = UrlHelper.BuildUrl(Url, "/api/Regional/postal-codes", query);

                    var json = await _transporter.Get(url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var postalCodes = JsonConvert.DeserializeObject<List<string>>(json);

                        if (!string.IsNullOrEmpty(startsWith))
                        {
                            foreach (var city in postalCodes)
                            {
                                if (city.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    result.Add(city);
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
            }

            return result;
        }

        public async Task<PostalCodeInfo> GetPostalCodeInfo(string postalCode)
        {
            PostalCodeInfo result = null;

            if (Region != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query.Add("countryCode", _countryCode);
                    query.Add("postalCode", postalCode);
                    query.Add("langCode", _langCode);

                    var url = UrlHelper.BuildUrl(Url, "/api/Regional/postal-code-info", query);

                    var json = await _transporter.Get(url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var postalCodeInfo = JsonConvert.DeserializeObject<PostalCodeInfo>(json);

                        if (postalCodeInfo!=null)
                        {
                            result = postalCodeInfo;
                        }
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - GetPostalCodeInfo", e);
                }
            }

            return result;
        }

        public async Task UpdateCitySuggestions(string text)
        {
            var suggestions = await GetCities(text);

            CitySuggestions = suggestions;
        }

        public async Task UpdateStreetSuggestions(string text)
        {
            var suggestions = await GetStreets(text);

            StreetSuggestions = suggestions;
        }

        public async Task<List<string>> GetCities(string text)
        {
            var result = new List<string>();

            if (Region != null)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);

                    query.Add("countryCode", _countryCode);
                    query.Add("regionCode", Region.ShortName);

                    var url = UrlHelper.BuildUrl(Url, "/api/Regional/cities", query);

                    var json = await _transporter.Get(url);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var cities = JsonConvert.DeserializeObject<List<string>>(json);

                        if (!string.IsNullOrEmpty(text))
                        {
                            foreach (var city in cities)
                            {
                                if (city.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
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
            }

            return result;
        }

        public async Task<List<string>> GetStreets(string text)
        {
            var result = new List<string>();
            var geoConnector = new GeoAdminConnector();

            var streetInfos = await geoConnector.GetStreetsInfo(text);

            if(streetInfos!=null)
            {
                foreach(var streetInfo in streetInfos)
                {
                    if(streetInfo.City == City || streetInfo.PostalCode == PostalCode)
                    {
                        result.Add(streetInfo.Street);
                    }
                }
            }

            return result;
        }

        private async Task<List<Region>> ReadRegions()
        {   
            var result = new List<Region>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("countryCode", _countryCode);
                query.Add("langCode", _langCode);

                var url = UrlHelper.BuildUrl(Url, "api/regional/regions", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<List<Region>>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadRegions", e);
            }

            return result;            
        }

        private async Task<bool> StoreContact()
        {
            Contact.Name = Name;
            Contact.Phone = Phone;
            Contact.Street = Street;

            if (Region != null)
            {
                Contact.Region = Region.Name;
            }

            Contact.City = City;
            Contact.PostalCode = PostalCode;
            Contact.Notice = Notice;

            await UpdateCoordinates();

            return await SetContact();
        }

        private async Task UpdateCoordinates()
        {
            if (!string.IsNullOrEmpty(Contact.City) || !string.IsNullOrEmpty(Contact.PostalCode))
            {
                var connector = new GeoAdminConnector();

                var coordinates = await connector.GetAddressCoordinates($"{Contact.PostalCode} {Contact.City} {Contact.Street}");

                foreach (var coordinate in coordinates)
                {
                    if (coordinate.IsValid)
                    {
                        Contact.Latitude = coordinate.Latitude;
                        Contact.Longitude = coordinate.Longitude;
                        break;
                    }
                }
            }
        }

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
                MsgLogger.Exception($"{GetType().Name} - SignOut", e);
            }

            return result;
        }

        public async Task<bool> SetContact()
        {
            bool result = false;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                var json = JsonConvert.SerializeObject(Contact);

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

        public override async void Hide()
        {
            await StoreContact();

            base.Hide();
        }

        private void OnLocateClicked()
        {
            Task.Run(async () => { await GetLocation(); });
        }

        private void OnPhoneChanged()
        {
            IsPhoneValid = Phone.IsPhoneNumber();
        }

        private async void OnPostalCodeChanged()
        {
            if(!string.IsNullOrEmpty(PostalCode))
            {
                var info = await GetPostalCodeInfo(PostalCode);

                if(info!=null)
                {
                    if(!string.IsNullOrEmpty(info.Region.ShortName) && info.Region.ShortName != Region.ShortName)
                    {
                        Region = FindRegion(info.Region.Name);

                        if (Region != null)
                        {
                            CitySuggestions = new List<string>();
                            City = string.Empty;
                        }
                    }

                    if(!string.IsNullOrEmpty(info.City) && info.City != City)
                    {
                        var citySuggestions = await GetCities(info.City);

                        CitySuggestions = citySuggestions;
                        City = info.City;
                    }
                }
            }
        }

        private async Task GetLocation()
        {
            try
            {
                IsBusy = true;

                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }

                IsBusy = false;
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
            catch (Exception)
            {
                // Unable to get location
            }

            /*if (Locations.Count > 0)
            {
                try
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(lat, lon);

                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        var geocodeAddress =
                            $"AdminArea:       {placemark.AdminArea}\n" +
                            $"CountryCode:     {placemark.CountryCode}\n" +
                            $"CountryName:     {placemark.CountryName}\n" +
                            $"FeatureName:     {placemark.FeatureName}\n" +
                            $"Locality:        {placemark.Locality}\n" +
                            $"PostalCode:      {placemark.PostalCode}\n" +
                            $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                            $"SubLocality:     {placemark.SubLocality}\n" +
                            $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                            $"Thoroughfare:    {placemark.Thoroughfare}\n";

                        Console.WriteLine(geocodeAddress);
                    }
                }
                catch (FeatureNotSupportedException)
                {
                    // Feature not supported on device
                }
                catch (Exception)
                {
                    // Handle exception that may have occurred in geocoding
                }
            }*/
        }

        #endregion
    }
}