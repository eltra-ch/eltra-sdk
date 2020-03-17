using EltraNavigo.Controls;
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
using System.Linq;

namespace EltraNavigo.Views.Contact
{
    public class ContactViewModel : ToolViewModel
    {
        #region Private fields

        private bool _isValid;
        private CloudTransporter _transporter;
        
        private string _name;
        private List<string> _regions;
        private string _phone;
        private string _street;
        private string _region;
        private string _city;
        private string _postalCode;
        private string _notice;

        #endregion

        #region Constructors

        public ContactViewModel()
        {
            Title = "Profile";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.adjustments_32px.png");
            IsMandatory = true;
            Uuid = "791AFBD3-E61D-4A0B-B35B-874D5A038E35";
            
            AddRegions();

            _transporter = new CloudTransporter();

            PropertyChanged += (sender, args) => { UpdateValidFlag(); }; 
        }

        #endregion

        #region Properties

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

        public string Region
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

        public string PostalCode
        {
            get => _postalCode;
            set => SetProperty(ref _postalCode, value);
        }

        public string Notice
        {
            get => _notice;
            set => SetProperty(ref _notice, value);
        }

        public List<string> Regions
        {
            get => _regions ?? (_regions = new List<string>());
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
            Region = string.Empty;
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
            IsValid = !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Phone);
        }

        public override async Task Show()
        {
            await ReadContact();

            UpdateValidFlag();

            await base.Show();
        }

        private void AddRegions()
        {
            var regions = new List<string>() { "Zürich", "Bern", "Luzern", "Uri", 
                "Schwyz", "Obwalden", "Nidwalden", "Glarus", "Zug", "Freiburg", "Solothurn",
                "Basel-Stadt", "Basel-Landschaft", "Schaffhausen", "Appenzel Ausserrhoden", "Appenzell Innerhoden",
                "St. Gallen", "Graubünden", "Aargau", "Thurgau", "Waadt", "Wallis", "Neuenburg", "Genf", "Jura"};

            Regions = regions;
            Region = regions[5];
        }

        private async Task ReadContact()
        {
            var contact = await GetContact();

            if (contact != null)
            {
                Name = contact.Name;
                Phone = contact.Phone;
                Street = contact.Street;
                Region = contact.Region;
                City = contact.City;
                PostalCode = contact.PostalCode;
                Notice = contact.Notice;
            }
        }

        private async Task<bool> StoreContact()
        {
            var contact = new EltraCloudContracts.Enka.Contacts.Contact();

            contact.Name = Name;
            contact.Phone = Phone;
            contact.Street = Street;
            contact.Region = Region;
            contact.City = City;
            contact.PostalCode = PostalCode;
            contact.Notice = Notice;

            return await StoreContact(contact);
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

        public async Task<bool> StoreContact(EltraCloudContracts.Enka.Contacts.Contact contact)
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

        public override async Task Hide()
        {
            await StoreContact();

            await base.Hide();
        }

        private void OnLocateClicked()
        {
            Task.Run(async () => { await GetLocation(); });
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