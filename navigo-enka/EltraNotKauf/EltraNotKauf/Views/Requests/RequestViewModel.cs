using EltraCloudContracts.Enka.Orders;
using EltraNotKauf.Controls;
using EltraNotKauf.Controls.Button;
using EltraNotKauf.Controls.Toast;
using EltraNotKauf.Endpoints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using EnkaOrders = EltraCloudContracts.Enka.Orders;

namespace EltraNotKauf.Views.Requests
{
    public class RequestViewModel : ToolViewBaseModel
    {
        #region Private fields

        private string _orderUuid;
        private OrderStatus _orderStatus;
        private List<OrderStatus> _orderStatusList;
        private JsonProtocolV1 _message;
        private string _orderModifiedTime;
        private string _orderRemainingTime;
        private string _createdBy;
        private string _orderCity;
        private string _orderPostalCode;
        private string _orderRegion;
        private string _orderStreet;
        private string _orderPhone;
        private string _description;
        private bool _requestChecked;
        private List<string> _assignedTo;

        private Timer _remainingTimer;
        private Timer _orderStatusTimer;

        private OrderInfo _orderInfo;
        private ThreeStateButtonViewModel _helpButtonViewModel;
        private OrdersEndpoint _ordersEndpoint;
        private ContactEndpoint _contactEndpoint;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;

        #endregion

        #region Constructors

        public RequestViewModel(ToolViewBaseModel parent, OrdersEndpoint ordersEndpoint, OrderInfo orderInfo)
            : base(parent)
        {
            _contactEndpoint = new ContactEndpoint();
            _ordersEndpoint = ordersEndpoint;
            _orderInfo = orderInfo;
            
            CreateOrderStatusList();

            if (_orderInfo != null)
            {
                _remainingTimer = new Timer();
                _orderStatusTimer = new Timer();

                UpdateOrderInfo(_orderInfo.Order);

                UpdateOrderContactInfo();

                UpdateContactInfo();

                UpdateRemainingTime();

                _remainingTimer.Interval = 1000;
                _remainingTimer.Elapsed += OnRemainingTimeElapsed;
                
                _orderStatusTimer.Interval = 5000;
                _orderStatusTimer.Elapsed += OnOrderStatusElapsed;
              
                HelpButtonViewModel.Height = 34;
                HelpButtonViewModel.Id = "help;";
                HelpButtonViewModel.Title = "Ich will helfen!";
                HelpButtonViewModel.ButtonStateChanged += OnHelpButtonStateChanged;
            }
        }

        private void OnOrderStatusElapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(async () =>
            {
                var orderInfo = await _ordersEndpoint.GetOrderInfo(_orderInfo.Order.Uuid);

                var newOrder = orderInfo?.Order;
                var oldOrder = _orderInfo?.Order;

                if(newOrder != null && oldOrder != null)
                {
                    if(newOrder.Modified != oldOrder.Modified)
                    {
                        _orderInfo.Order.Modified = newOrder.Modified;
                        _orderInfo.Order.Status = newOrder.Status;
                        _orderInfo.Order.Message = newOrder.Message;
                        _orderInfo.Order.Protocol = newOrder.Protocol;

                        UpdateOrderInfo(_orderInfo.Order);

                        UpdateAssignedInfo();
                    }
                }
            });
        }

        #endregion

        #region Properties

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public List<string> AssignedTo
        {
            get => _assignedTo;
            set => SetProperty(ref _assignedTo, value);
        }

        public string OrderPhone
        {
            get => _orderPhone;
            set => SetProperty(ref _orderPhone, value);
        }

        public string OrderCity
        {
            get => _orderCity;
            set => SetProperty(ref _orderCity, value);
        }

        public string OrderPostalCode
        {
            get => _orderPostalCode;
            set => SetProperty(ref _orderPostalCode, value);
        }

        public string OrderRegion
        {
            get => _orderRegion;
            set => SetProperty(ref _orderRegion, value);
        }

        public string OrderStreet
        {
            get => _orderStreet;
            set => SetProperty(ref _orderStreet, value);
        }

        public string OrderUuid 
        { 
            get => _orderUuid; 
            set => SetProperty(ref _orderUuid, value); 
        }

        public OrderStatus OrderStatus 
        { 
            get => _orderStatus; 
            set => SetProperty(ref _orderStatus, value);
        }

        public List<OrderStatus> OrderStatusList 
        { 
            get => _orderStatusList; 
            set => SetProperty(ref _orderStatusList, value);
        }

        public string OrderModifiedTime 
        { 
            get => _orderModifiedTime; 
            set => SetProperty(ref _orderModifiedTime, value); 
        }

        public string OrderRemainingTime
        {
            get => _orderRemainingTime;
            set => SetProperty(ref _orderRemainingTime, value);
        }

        public string CreatedBy 
        { 
            get => _createdBy; 
            set => SetProperty(ref _createdBy, value); 
        }

        public bool RequestChecked
        {
            get => _requestChecked;
            set => SetProperty(ref _requestChecked, value);
        }

        public ThreeStateButtonViewModel HelpButtonViewModel
        {
            get => _helpButtonViewModel ?? (_helpButtonViewModel = new ThreeStateButtonViewModel());
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

        #region Events

        private void OnRemainingTimeElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateRemainingTime();
        }

        private void OnHelpButtonStateChanged(object sender, EventArgs e)
        {
            if(HelpButtonViewModel.ButtonState == ButtonState.Active)
            {
                HelpButtonViewModel.Title = "Ich helfe!";

                ChangeOrderStatus(EnkaOrders.OrderStatus.Assigned);

            }
            else if (HelpButtonViewModel.ButtonState == ButtonState.Inactive)
            {
                HelpButtonViewModel.Title = "Ich will helfen!";

                ChangeOrderStatus(EnkaOrders.OrderStatus.Rejected);
            }
        }

        #endregion

        #region Methods

        private void UpdateContactInfo()
        {
            var task = Task.Run(async () =>
            {
                _contact = await _contactEndpoint.GetContact();
            });

            task.ContinueWith((t) => { UpdateAssignedInfo(); });
            
        }

        public override void Show()
        {
            UpdateContactInfo();

            _orderStatusTimer.Start();
            _remainingTimer.Start();

            base.Show();
        }

        public override void Hide()
        {
            _orderStatusTimer.Stop();
            _remainingTimer.Stop();

            base.Hide();
        }

        private void ChangeOrderStatus(EnkaOrders.OrderStatus orderStatus)
        {
            Task.Run(async () =>
            {
                IsBusy = true;

                if (_orderInfo != null)
                {
                    _orderInfo.Order.Status = orderStatus;

                    if (await _ordersEndpoint.ChangeOrder(_orderInfo.Order))
                    {
                        ToastMessage.ShortAlert($"Bravo! Aufgabe {_orderInfo.Order.Uuid} aufgenommen!");
                    }
                    else
                    {
                        ToastMessage.ShortAlert($"Aufgabe {_orderInfo.Order.Uuid} leider nicht aufgenommen!");
                    }
                }

                IsBusy = false;
            });
        }

        private void UpdateRemainingTime()
        {
            if (_orderInfo != null)
            {
                var durationInSec = (int)(DateTime.Now - _orderInfo.Order.Modified).TotalSeconds;
                var timeout = _orderInfo.Order.Timeout;
                var remainingSec = timeout - durationInSec;

                TimeSpan remains = new TimeSpan(remainingSec * TimeSpan.TicksPerSecond);

                var totalDays = (int)Math.Round(remains.TotalDays);
                var totalHours = (int)Math.Round(remains.TotalHours);
                var totalMinutes = (int)Math.Round(remains.TotalMinutes); 
                var totalSeconds = (int)Math.Round(remains.TotalSeconds);
                
                if (totalDays > 0)
                {
                    if (totalDays == 1)
                    {
                        OrderRemainingTime = $"{totalDays} Tag";
                    }
                    else
                    {
                        OrderRemainingTime = $"{totalDays} Tage";
                    }
                }
                else if (totalHours > 0)
                {
                    if (totalHours == 1)
                    {
                        OrderRemainingTime = $"{totalHours} Stunde";
                    }
                    else
                    {
                        OrderRemainingTime = $"{totalHours} Stunden";
                    }
                }
                else if (totalMinutes > 0)
                {
                    if (totalMinutes == 1)
                    {
                        OrderRemainingTime = $"{totalMinutes} Minute";
                    }
                    else
                    {
                        OrderRemainingTime = $"{totalMinutes} Minuten";
                    }
                }
                else if (totalSeconds > 0)
                {
                    if (totalSeconds == 1)
                    {
                        OrderRemainingTime = $"{totalSeconds} Sekunde";
                    }
                    else
                    {
                        OrderRemainingTime = $"{totalSeconds} Sekunden";
                    }
                }
            }
        }

        private void UpdateOrderInfo(Order order)
        {
            if (order != null)
            {
                OrderUuid = order.Uuid;
                OrderModifiedTime = order.Modified.ToShortTimeString();

                UpdateMessage(order);

                UpdateOrderStatus(order);                
            }
        }

        private void UpdateAssignedInfo()
        {
            var assignedTo = _orderInfo.AssignedTo;

            if (assignedTo != null && _contact != null)
            {
                var assignedToList = new List<string>();

                foreach (var contact in assignedTo)
                {
                    if (contact.Uuid != _contact.Uuid)
                    {
                        assignedToList.Add(contact.Name);
                    }
                    else
                    {
                        HelpButtonViewModel.ButtonState = ButtonState.Active;
                        HelpButtonViewModel.Title = "Ich helfe!";
                    }
                }

                AssignedTo = assignedToList;
            }
        }

        private void UpdateOrderContactInfo()
        {
            var contact = _orderInfo.CreatedBy;

            if (contact != null)
            {
                OrderCity = contact.City;
                OrderPostalCode = contact.PostalCode;
                OrderRegion = contact.Region;
                OrderStreet = contact.Street;

                OrderPhone = contact.Phone;

                CreatedBy = contact.Name;
            }
        }

        private void UpdateMessage(Order order)
        {
            if (order.Protocol == "json_v1")
            {
                _message = JsonConvert.DeserializeObject<JsonProtocolV1>(order.Message);

                if(_message!=null)
                {
                    if(_message.Car)
                    {
                        Description += "AUTO";
                        Description += ", ";
                    }

                    if (_message.DrugStore)
                    {
                        Description += "APOTHEKE";
                        Description += ", ";
                    }

                    if (_message.Shop)
                    {
                        Description += "LADEN";
                        Description += ", ";
                    }

                    if (_message.Other)
                    {
                        Description += "ANDERE";
                        Description += ", ";
                    }

                    if (!string.IsNullOrEmpty(_message.Notice))
                    {
                        Description += _message.Notice;
                    }

                    Description = Description.TrimEnd();

                    if (Description.EndsWith(","))
                    {
                        Description = Description.Substring(0, Description.Length - 1);
                    }
                }
            }
        }

        private void UpdateOrderStatus(Order order)
        {
            switch (order.Status)
            {
                case EnkaOrders.OrderStatus.Assigned:
                    OrderStatus = OrderStatusList[2];
                    break;
                case EnkaOrders.OrderStatus.Open:
                    OrderStatus = OrderStatusList[1];
                    break;
                case EnkaOrders.OrderStatus.Expired:
                    OrderStatus = OrderStatusList[5];
                    break;
                case EnkaOrders.OrderStatus.Rejected:
                    OrderStatus = OrderStatusList[4];
                    break;
                case EnkaOrders.OrderStatus.Closed:
                    OrderStatus = OrderStatusList[3];
                    break;
                case EnkaOrders.OrderStatus.Undefined:
                    OrderStatus = OrderStatusList[0];
                    break;
            }
        }

        private void CreateOrderStatusList()
        {
            var orderStatusList = new List<OrderStatus>();

            orderStatusList.Add(new OrderStatus("Nicht definiert"));
            orderStatusList.Add(new OrderStatus("offen"));
            orderStatusList.Add(new OrderStatus("zugewiesen"));
            orderStatusList.Add(new OrderStatus("geschlossen"));
            orderStatusList.Add(new OrderStatus("abgelehnt"));
            orderStatusList.Add(new OrderStatus("abgelaufen"));

            OrderStatusList = orderStatusList;
        }

        #endregion
    }
}
