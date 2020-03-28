using EltraNotKauf.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.Generic;
using Region = EltraCloudContracts.Enka.Regional.Region;
using EltraNotKauf.Endpoints;
using System;
using System.Web;
using EltraCommon.Helpers;
using Newtonsoft.Json;
using EltraCommon.Logger;
using EltraConnector.Transport;
using EltraCloudContracts.Enka.Orders;
using System.Linq;

namespace EltraNotKauf.Views.Requests
{
    public class RequestsViewModel : ToolViewModel
    {
        #region Private fields

        private RegionEndpoint _regionEndpoint;
        private CloudTransporter _transporter;

        private List<Region> _regions;
        private string _street;
        private Region _region;
        private string _city;
        private List<string> _citySuggestions;
        private string _postalCode;
        private List<string> _postalCodeSuggestions;
        private List<string> _streetSuggestions;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;
        private List<RequestViewModel> _requestList;

        #endregion

        #region Constructors

        public RequestsViewModel()
        {
            Title = "Anfragen";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.lifebuoy.png");
            IsMandatory = true;
            Uuid = "B35E33E8-351D-44B9-B169-134AD4566F48";
            
            _regionEndpoint = new RegionEndpoint();
            _transporter = new CloudTransporter();

            PropertyChanged += (sender, args) => 
            { 
                if(args.PropertyName == "PostalCode")
                {
                    OnPostalCodeChanged();
                }
            }; 
        }

        #endregion

        #region Properties

        public string CountryCode => "CH";

        public Region Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
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
        
        public List<Region> Regions
        {
            get => _regions ?? (_regions = new List<Region>());
            set => SetProperty(ref _regions, value);
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

        public EltraCloudContracts.Enka.Contacts.Contact Contact
        {
            get => _contact ?? (_contact = new EltraCloudContracts.Enka.Contacts.Contact());
            set => _contact = value;
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

        public List<RequestViewModel> RequestList
        {
            get => _requestList ?? (_requestList = new List<RequestViewModel>());
            set => SetProperty(ref _requestList, value);
        }
        
        #endregion

        #region Command

        public ICommand SearchCommand => new Command(OnSearchCommandClicked);
        
        #endregion

        #region Methods

        public override void Clear()
        {
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
        }

        private async void OnSearchCommandClicked(object obj)
        {
            await UpdateRequests();
        }

        private async Task UpdateRequests()
        {
            var orderInfoList = await GetOrderInfoList();
            var requestList = new List<RequestViewModel>();

            foreach (var orderInfo in orderInfoList)
            {
                var requestViewModel = new RequestViewModel(orderInfo);

                requestList.Add(requestViewModel);
            }

            RequestList = requestList;
        }

        public override void Show()
        {
            Task.Run(async () =>
            {
                IsBusy = true;

                Regions = await _regionEndpoint.ReadRegions();

                ReadContact();

                await UpdateRequests();

                IsBusy = false;
            });

            base.Show();
        }

        private async void ReadContact()
        {
            var contact = await GetContact();

            if (contact != null)
            {
                Contact = contact;

                Region = FindRegion(Contact.Region);
            }
            else
            {
                Contact = null;

                Region = Regions.FirstOrDefault();
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
                MsgLogger.Exception($"{GetType().Name} - GetContact", e);
            }

            return result;
        }

        public async Task<List<OrderInfo>> GetOrderInfoList()
        {
            var result = new List<OrderInfo>(); 

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("country", CountryCode);

                if (Region != null && !string.IsNullOrEmpty(Region.Name))
                {
                    query.Add("region", Region.Name);
                }
                
                if (!string.IsNullOrEmpty(City))
                {
                    query.Add("city", City);
                }

                var url = UrlHelper.BuildUrl(Url, "api/Orders/get-all-region", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonConvert.DeserializeObject<List<OrderInfo>>(json);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetOrders", e);
            }

            return result;
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