using EltraNotKauf.Controls;
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
using EltraNotKauf.Controls.Button;

namespace EltraNotKauf.Views.Orders
{
    public class OrderViewModel : ToolViewModel
    {
        #region Private fields

        private const double OrderUpdateInterval = 5000;

        private bool _isValid;
        private CloudTransporter _transporter;
        private Order _activeOrder;
        
        private ThreeStateButtonViewModel _otherButtonViewModel;
        private ThreeStateButtonViewModel _shopButtonViewModel;
        private ThreeStateButtonViewModel _drugStoreButtonViewModel;
        private ThreeStateButtonViewModel _carButtonViewModel;

        private string _activeOrderStatus;
        private string _activeOrderTitle;
        private Timer _timer;

        #endregion

        #region Constructors

        public OrderViewModel()
        {
            Title = "Anfragen";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.urgent.png");
            IsMandatory = true;
            Uuid = "37A00C5A-3A87-40F5-B954-5BE2161728F2";

            _transporter = new CloudTransporter();
            _timer = new Timer(OrderUpdateInterval);
            _timer.Elapsed += OnUpdateIntervalElapsed;

            OtherButtonViewModel.Id = "other;";
            OtherButtonViewModel.Title = "ANDERE";

            ShopButtonViewModel.Id = "shop;";
            ShopButtonViewModel.Title = "LADEN";

            CarButtonViewModel.Id = "car;";
            CarButtonViewModel.Title = "AUTO";

            DrugStoreButtonViewModel.Id = "drug_store;";
            DrugStoreButtonViewModel.Title = "APOTHEKE";

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

        public Order ActiveOrder
        {
            get => _activeOrder;
            set => SetProperty(ref _activeOrder, value);
        }

        public string ActiveOrderStatus
        {
            get => _activeOrderStatus;
            set => SetProperty(ref _activeOrderStatus, value);
        }

        public string ActiveOrderTitle
        {
            get => _activeOrderTitle;
            set => SetProperty(ref _activeOrderTitle, value);
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

        private async void OnUpdateIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            ActiveOrder = await GetActiveOrder();

            UpdateActiveOrderStatus();
            UpdateButtonsState();
        }

        private async void OnButtonStateChanged(object sender, EventArgs e)
        {
            string json = string.Empty;
            var message = new JsonProtocolV1();

            ActiveOrder = await GetActiveOrder();

            if (OtherButtonViewModel.ButtonState == ButtonState.Active)
            {
                message.Other = true;
            }

            if (ShopButtonViewModel.ButtonState == ButtonState.Active)
            {
                message.Shop = true;
            }

            if (CarButtonViewModel.ButtonState == ButtonState.Active)
            {
                message.Car = true;
            }

            if (DrugStoreButtonViewModel.ButtonState == ButtonState.Active)
            {
                message.DrugStore = true;
            }

            json = JsonConvert.SerializeObject(message);

            if (string.IsNullOrEmpty(json))
            {
                if (ActiveOrder != null)
                {
                    if(ActiveOrder.Status == OrderStatus.Assigned || ActiveOrder.Status == OrderStatus.Open)
                    {
                        ActiveOrder.Status = OrderStatus.Closed;

                        if (await ChangeOrder(ActiveOrder))
                        {
                            ActiveOrder = await GetActiveOrder();

                            UpdateActiveOrderStatus();
                        }
                        else
                        {
                            ActiveOrderStatus = "Fehler :-(";
                        }
                    }
                }
            }
            else
            {
                if (ActiveOrder != null)
                {
                    if (ActiveOrder.Status == OrderStatus.Closed)
                    {
                        if (await AddOrder(new Order() { Message = json }))
                        {
                            ActiveOrder = await GetActiveOrder();

                            UpdateActiveOrderStatus();
                        }
                        else
                        {
                            ActiveOrderStatus = "Fehler :-(";
                        }
                    }
                    else
                    {
                        if (ActiveOrder.Status == OrderStatus.Rejected)
                        {
                            ActiveOrder.Status = OrderStatus.Open;
                        }

                        ActiveOrder.Message = json;

                        if (await ChangeOrder(ActiveOrder))
                        {
                            ActiveOrder = await GetActiveOrder();

                            UpdateActiveOrderStatus();
                        }
                        else
                        {
                            ActiveOrderStatus = "Fehler :-(";
                        }
                    }
                }
                else
                {
                    if (await AddOrder(new Order() { Message = json }))
                    {
                        ActiveOrder = await GetActiveOrder();

                        UpdateActiveOrderStatus();
                    }
                    else
                    {
                        ActiveOrderStatus = "Fehler :-(";
                    }
                }
            }
        }
        
        #endregion

        #region Methods

        private void UpdateActiveOrderStatus()
        {
            if (ActiveOrder != null)
            {
                if (ActiveOrder.Status == OrderStatus.Open)
                {
                    ActiveOrderStatus = "Erstellt. Bitte warten.";
                }
                else if (ActiveOrder.Status == OrderStatus.Assigned)
                {
                    ActiveOrderStatus = "in Bearbeitung";
                }
                else if (ActiveOrder.Status == OrderStatus.Closed)
                {
                    ActiveOrderStatus = "Erledigt";
                }
                else
                {
                    ActiveOrderStatus = "";
                }

                ActiveOrderTitle = ActiveOrder.Uuid;
            }
            else
            {
                ActiveOrderStatus = "noch keinen Auftrag platziert";
                ActiveOrderTitle = "";
            }
        }

        public async Task<Order> GetActiveOrder()
        {
            Order result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                var url = UrlHelper.BuildUrl(Url, "/api/orders/get", query);

                var response = await _transporter.Get(url);

                if(!string.IsNullOrEmpty(response))
                {
                    var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                    foreach(var order in orders)
                    {
                        if(order.Status == OrderStatus.Open)
                        {
                            result = order;
                            break;
                        }
                        else if(order.Status == OrderStatus.Assigned)
                        {
                            result = order;
                            break;
                        }
                        else if(order.Status == OrderStatus.Closed)
                        {
                            result = order;
                            break;
                        }                        
                    }
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
            _timer.Start();

            ActiveOrder = await GetActiveOrder();

            UpdateActiveOrderStatus();
            UpdateButtonsState();

            await base.Show();
        }

        private void UpdateButtonsState()
        {
            try
            {
                if (ActiveOrder != null && !string.IsNullOrEmpty(ActiveOrder.Message))
                {
                    var jsonV1 = JsonConvert.DeserializeObject<JsonProtocolV1>(ActiveOrder.Message);

                    OtherButtonViewModel.ButtonState = jsonV1.Other ? ButtonState.Active : ButtonState.Inactive;
                    CarButtonViewModel.ButtonState = jsonV1.Car ? ButtonState.Active : ButtonState.Inactive;
                    ShopButtonViewModel.ButtonState = jsonV1.Shop ? ButtonState.Active : ButtonState.Inactive;
                    DrugStoreButtonViewModel.ButtonState = jsonV1.DrugStore ? ButtonState.Active : ButtonState.Inactive;
                }
                else
                {
                    DrugStoreButtonViewModel.ButtonState = ButtonState.Inactive;
                    OtherButtonViewModel.ButtonState = ButtonState.Inactive;
                    ShopButtonViewModel.ButtonState = ButtonState.Inactive;
                    CarButtonViewModel.ButtonState = ButtonState.Inactive;
                }
            }
            catch(Exception)
            {
            }
        }

        public override Task Hide()
        {
            _timer.Stop();

            return base.Hide();
        }

        public async Task<bool> AddOrder(Order order)
        {
            bool result = false;

            try
            {
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