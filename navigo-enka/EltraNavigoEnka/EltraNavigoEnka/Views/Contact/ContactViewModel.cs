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
        private List<Location> _locations;

        private string _firstName;
        private string _lastName;
        private string _phoneStationary;
        private string _phoneMobile;
        private string _street;
        private string _houseNumber;
        private string _flatNumber;
        private string _city;
        private string _postalCode;
        private string _notice;

        #endregion

        #region Constructors

        public ContactViewModel()
        {
            Title = "Contact";
            Image = ImageSource.FromResource("EltraNavigo.Resources.lightbulb_32px.png");
            IsMandatory = true;
            Uuid = "791AFBD3-E61D-4A0B-B35B-874D5A038E35";

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

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string PhoneStationary
        {
            get => _phoneStationary;
            set => SetProperty(ref _phoneStationary, value);
        }

        public string PhoneMobile
        {
            get => _phoneMobile;
            set => SetProperty(ref _phoneMobile, value);
        }

        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set => SetProperty(ref _houseNumber, value);
        }

        public string FlatNumber
        {
            get => _flatNumber;
            set => SetProperty(ref _flatNumber, value);
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

        public List<Location> Locations
        {
            get => _locations ?? (_locations = new List<Location>());
            set => SetProperty(ref _locations, value);
        }

        #endregion

        #region Command

        public ICommand SaveCommand => new Command(OnSaveCommandClicked);
        public ICommand LocateCommand => new Command(OnLocateClicked);

        #endregion

        #region Methods

        private async void OnSaveCommandClicked(object obj)
        {
            await StoreContact();
        }

        private void UpdateValidFlag()
        {
            IsValid = !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName);
        }

        public override async Task Show()
        {
            await ReadContact();

            UpdateValidFlag();

            await base.Show();
        }

        private async Task ReadContact()
        {
            var contact = await GetContact();

            if (contact != null)
            {
                FirstName = contact.FirstName;
                LastName = contact.LastName;
                PhoneStationary = contact.PhoneStationary;
                PhoneMobile = contact.PhoneMobile;
                Street = contact.Street;
                HouseNumber = contact.HouseNumber;
                FlatNumber = contact.FlatNumber;
                City = contact.City;
                PostalCode = contact.PostalCode;
                Notice = contact.Notice;
            }
        }

        private async Task<bool> StoreContact()
        {
            var contact = new EltraCloudContracts.Enka.Contacts.Contact();

            contact.FirstName = FirstName;
            contact.LastName = LastName;
            contact.PhoneStationary = PhoneStationary;
            contact.PhoneMobile = PhoneMobile;
            contact.Street = Street;
            contact.HouseNumber = HouseNumber;
            contact.FlatNumber = FlatNumber;
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

                Locations = new List<Location>() { location };
                
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

            if (Locations.Count > 0)
            {
                try
                {
                    var lat = Locations[0].Latitude;
                    var lon = Locations[0].Longitude;

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
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Feature not supported on device
                }
                catch (Exception ex)
                {
                    // Handle exception that may have occurred in geocoding
                }
            }
        }

        #endregion
    }
}