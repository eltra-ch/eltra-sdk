using EltraNotKauf.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraCommon.Helpers;
using System.Collections.Generic;
using Region = EltraCloudContracts.Enka.Regional.Region;
using EltraConnector.GeoAdmin;
using EltraNotKauf.Endpoints;
using EltraNotKauf.Controls.Toast;
using System.Reflection;
using System.Linq;

namespace EltraNotKauf.Views.Contact
{
    public class ContactViewModel : ToolViewModel
    {
        #region Private fields

        private bool _isValid;
        private RegionEndpoint _regionEndpoint;
        private ContactEndpoint _contactEndpoint;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;

        private string _contactUuid;
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

        private bool _isPhoneValid;

        #endregion

        #region Constructors

        public ContactViewModel()
        {
            Title = "Addresse";
            
            
            Image = ImageSource.FromResource("EltraNotKauf.Resources.home.png", Assembly.GetExecutingAssembly());
            IsMandatory = true;
            Uuid = "791AFBD3-E61D-4A0B-B35B-874D5A038E35";
            IsPhoneValid = true;

            _regionEndpoint = new RegionEndpoint();
            _contactEndpoint = new ContactEndpoint();

            PropertyChanged += (sender, args) => 
            {
                if (IsVisible)
                {
                    if (args.PropertyName == "PostalCode")
                    {
                        OnPostalCodeChanged();
                    }
                    if (args.PropertyName == "Phone")
                    {
                        OnPhoneChanged();
                    }

                    UpdateValidFlag();
                }
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

        internal async Task<List<string>> GetPostalCodes(string text)
        {
            var result = new List<string>();

            if (Region != null)
            {
                result = await _regionEndpoint.GetPostalCodes(Region.ShortName, text);
            }

            return result;
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

        public string ContactUuid
        {
            get => _contactUuid;
            set => SetProperty(ref _contactUuid, value);
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
        
        #endregion

        #region Methods

        public override void Reset()
        {
            ContactUuid = string.Empty;
            Name = string.Empty;
            Phone = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            Notice = string.Empty;
        }

        private async void OnSaveCommandClicked(object obj)
        {
            IsBusy = true;

            if(await StoreContact())
            {
                ToastMessage.ShortAlert("Gespeichert");
            }
            else
            {
                ToastMessage.ShortAlert("Fehlgeschlagen!");
            }

            IsBusy = false;
        }

        private void UpdateValidFlag()
        {
            IsValid = !string.IsNullOrEmpty(Name) && IsPhoneValid;            
        }

        public override async void Show()
        {
            IsBusy = true;

            Regions = await _regionEndpoint.ReadRegions();

            if (Regions.Count > 0)
            {
                Region = Regions.FirstOrDefault();
            }

            ReadContact();

            UpdateValidFlag();

            IsBusy = false;

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
        
        private async void ReadContact()
        {
            var contact = await _contactEndpoint.GetContact();

            if (contact != null)
            {
                Contact = contact;

                if (!string.IsNullOrEmpty(Contact.Uuid))
                {
                    ContactUuid = Contact.Uuid;
                }

                if (!string.IsNullOrEmpty(Contact.Name))
                {
                    Name = Contact.Name;
                }

                if (!string.IsNullOrEmpty(Contact.Phone))
                {
                    Phone = Contact.Phone;
                }

                if (!string.IsNullOrEmpty(Contact.Street))
                {
                    Street = Contact.Street;
                }

                if (Contact.Region != null)
                {
                    Region = FindRegion(Contact.Region);
                }

                if (!string.IsNullOrEmpty(Contact.City))
                {
                    City = Contact.City;
                }

                if (!string.IsNullOrEmpty(Contact.City))
                {
                    PostalCode = Contact.PostalCode;
                }

                if (!string.IsNullOrEmpty(Contact.City))
                {
                    Notice = Contact.Notice;
                }
            }
        }

        public async Task UpdateCitySuggestions(string text)
        {
            if (Region != null)
            {
                var suggestions = await _regionEndpoint.GetCities(Region.ShortName, text);

                CitySuggestions = suggestions;
            }
        }

        public async Task UpdateStreetSuggestions(string text)
        {
            var suggestions = await _regionEndpoint.GetStreets(City, PostalCode, text);

            StreetSuggestions = suggestions;
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

            return await _contactEndpoint.SetContact(Contact);
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
        
        public override async void Hide()
        {
            await StoreContact();

            base.Hide();
        }

        private void OnPhoneChanged()
        {
            IsPhoneValid = Phone.IsPhoneNumber();
        }

        private async void OnPostalCodeChanged()
        {
            if(!string.IsNullOrEmpty(PostalCode))
            {
                var info = await _regionEndpoint.GetPostalCodeInfo(PostalCode);

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
                        var citySuggestions = await _regionEndpoint.GetCities(info.Region.ShortName, info.City);

                        CitySuggestions = citySuggestions;
                        City = info.City;
                    }
                }
            }
        }

        #endregion
    }
}