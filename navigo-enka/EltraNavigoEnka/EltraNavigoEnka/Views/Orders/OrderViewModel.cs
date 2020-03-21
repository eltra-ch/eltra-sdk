using EltraNavigo.Controls;
using System.Threading.Tasks;
using Xamarin.Forms;
using EltraConnector.Transport;
using System;
using EltraCommon.Logger;
using Newtonsoft.Json;
using EltraCloudContracts.Enka.Orders;
using System.Collections.Generic;
using EltraCommon.Helpers;
using System.Web;
using System.Timers;
using EltraNavigoEnka.Controls.Button;

namespace EltraNavigo.Views.Orders
{
    public class OrderViewModel : ToolViewModel
    {
        #region Private fields

        private const double OrderUpdateInterval = 5000;

        private bool _isValid;
        private CloudTransporter _transporter;
        private List<Order> _orderList;
        
        private ThreeStateButtonViewModel _otherButtonViewModel;
        private ThreeStateButtonViewModel _shopButtonViewModel;
        private ThreeStateButtonViewModel _drugStoreButtonViewModel;
        private ThreeStateButtonViewModel _carButtonViewModel;

        #endregion

        #region Constructors

        public OrderViewModel()
        {
            Title = "Orders";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.book-open_32px.png");
            IsMandatory = true;
            Uuid = "37A00C5A-3A87-40F5-B954-5BE2161728F2";

            _transporter = new CloudTransporter();
            
            OtherButtonViewModel.Label = "ANDERE";
            ShopButtonViewModel.Label = "LADEN";
            CarButtonViewModel.Label = "AUTO";
            DrugStoreButtonViewModel.Label = "APOTHEKE";

            OtherButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            ShopButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            CarButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            DrugStoreButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
        }

        #endregion

        #region Properties

        public ThreeStateButtonViewModel OtherButtonViewModel
        {
            get => _otherButtonViewModel ?? (_otherButtonViewModel = new ThreeStateButtonViewModel());
        }

        public ThreeStateButtonViewModel CarButtonViewModel
        {
            get => _carButtonViewModel ?? (_carButtonViewModel = new ThreeStateButtonViewModel());
        }

        public ThreeStateButtonViewModel ShopButtonViewModel
        {
            get => _shopButtonViewModel ?? (_shopButtonViewModel = new ThreeStateButtonViewModel());
        }
        public ThreeStateButtonViewModel DrugStoreButtonViewModel
        {
            get => _drugStoreButtonViewModel ?? (_drugStoreButtonViewModel = new ThreeStateButtonViewModel());
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public List<Order> OrderList
        {
            get => _orderList;
            set => SetProperty(ref _orderList, value);
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

        #endregion

        #region Command

        #endregion

        #region Events

        private async void OnButtonStateChanged(object sender, EventArgs e)
        {
            string order = string.Empty;

            OrderList = await GetOrders();

            if (OtherButtonViewModel.ButtonState == ButtonState.Active)
            {
                order += "other;";
            }

            if (ShopButtonViewModel.ButtonState == ButtonState.Active)
            {
                order += "shop;";
            }

            if (CarButtonViewModel.ButtonState == ButtonState.Active)
            {
                order += "car;";
            }

            if (DrugStoreButtonViewModel.ButtonState == ButtonState.Active)
            {
                order += "drug_store;";
            }

            if(OrderList.Count > 0)
            {
                var orderEntry = OrderList[0];

                orderEntry.Notice = order;

                await ChangeOrder(orderEntry);
            }
            else
            {
                await AddOrder(new Order() { Notice = order });
            }
        }
        
        #endregion

        #region Methods

        public async Task<List<Order>> GetOrders()
        {
            var result = new List<Order>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                var url = UrlHelper.BuildUrl(Url, "/api/orders/get", query);

                var response = await _transporter.Get(url);

                if(!string.IsNullOrEmpty(response))
                {
                    result = JsonConvert.DeserializeObject<List<Order>>(response);
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetOrders", e);
            }

            return result;
        }

        public override async Task Show()
        {
            OrderList = await GetOrders();

            foreach(var order in OrderList)
            {
                order.Notice.Contains("");
            }

            await base.Show();
        }

        public async Task<bool> AddOrder(Order order)
        {
            bool result = false;

            try
            {
                order.Type = OrderType.Request;

                var json = JsonConvert.SerializeObject(order);

                var response = await _transporter.Post(Url, "api/orders/add", json);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - StoreOrder", e);
            }

            return result;
        }

        public async Task<bool> ChangeOrder(Order order)
        {
            bool result = false;

            try
            {
                order.Type = OrderType.Request;

                var json = JsonConvert.SerializeObject(order);

                var response = await _transporter.Post(Url, "api/orders/change", json);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ChangeOrder", e);
            }

            return result;
        }

        #endregion
    }
}