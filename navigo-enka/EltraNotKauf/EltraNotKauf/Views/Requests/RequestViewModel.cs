using EltraCloudContracts.Enka.Orders;
using EltraNotKauf.Controls;
using EltraNotKauf.Controls.Button;
using EltraNotKauf.Controls.Toast;
using EltraNotKauf.Endpoints;
using EltraNotKauf.Helpers;
using EltraNotKauf.Views.Common;
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
        private OrderStatusLabel _orderStatusLabel;
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
        private List<AssignedToViewModel> _assignedTo;

        private Timer _remainingTimer;
        private Timer _orderStatusTimer;

        private OrderInfo _orderInfo;
        private ThreeStateButtonViewModel _helpButtonViewModel;
        private OrdersEndpoint _ordersEndpoint;
        private ContactEndpoint _contactEndpoint;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;
        private Order _activeOrder;
        private OrderStatus _activeOrderStatus;

        #endregion

        #region Constructors

        public RequestViewModel(ToolViewBaseModel parent, OrdersEndpoint ordersEndpoint, OrderInfo orderInfo)
            : base(parent)
        {
            _contactEndpoint = new ContactEndpoint();
            _ordersEndpoint = ordersEndpoint;
            _orderInfo = orderInfo;
            
            if (_orderInfo != null)
            {
                _remainingTimer = new Timer();
                _orderStatusTimer = new Timer();

                ActiveOrder = _orderInfo.Order;

                UpdateOrderInfo();

                UpdateOrderContactInfo();

                UpdateContactInfo();

                UpdateRemainingTime();

                UpdateOrderStatus();

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
                if (ActiveOrder != null)
                {
                    var orderInfo = await _ordersEndpoint.GetOrderInfo(ActiveOrder.Uuid);

                    var newOrder = orderInfo?.Order;
                    var oldOrder = ActiveOrder;

                    if (newOrder != null && oldOrder != null)
                    {
                        if (newOrder.Modified != oldOrder.Modified)
                        {
                            _orderInfo = orderInfo;

                            ActiveOrder = newOrder;

                            UpdateOrderInfo();

                            UpdateAssignedInfo();
                        }
                    }
                }
            });
        }

        #endregion

        #region Properties

        public Order ActiveOrder
        {
            get => _activeOrder;
            set => SetProperty(ref _activeOrder, value);
        }

        public OrderStatus ActiveOrderStatus
        {
            get => _activeOrderStatus;
            set => SetProperty(ref _activeOrderStatus, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public List<AssignedToViewModel> AssignedTo
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

        public OrderStatusLabel OrderStatusLabel 
        { 
            get => _orderStatusLabel; 
            set => SetProperty(ref _orderStatusLabel, value);
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
                HelpButtonViewModel.Title = "Ich bin dabei!";

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

                if (ActiveOrder != null)
                {
                    ActiveOrder.Status = orderStatus;

                    if (await _ordersEndpoint.ChangeOrder(ActiveOrder))
                    {
                        ThreadHelper.RunOnMainThread(()=> 
                        {
                            ToastMessage.ShortAlert($"Aufgabe {ActiveOrder.Uuid} aufgenommen!");
                        });
                        
                    }
                    else
                    {
                        ThreadHelper.RunOnMainThread(() =>
                        {
                            ToastMessage.ShortAlert($"Aufgabe {ActiveOrder.Uuid} leider nicht aufgenommen!");
                        });
                    }
                }

                IsBusy = false;
            });
        }

        private void UpdateRemainingTime()
        {
            if (ActiveOrder != null)
            {
                var durationInSec = (int)(DateTime.Now - ActiveOrder.Modified).TotalSeconds;
                var timeout = ActiveOrder.Timeout;
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

        private void UpdateOrderInfo()
        {
            if (ActiveOrder != null)
            {
                OrderUuid = ActiveOrder.Uuid;
                OrderModifiedTime = ActiveOrder.Modified.ToShortTimeString();

                UpdateMessage();

                UpdateOrderStatus();                
            }
        }

        private void UpdateAssignedInfo()
        {
            var assignedTo = _orderInfo.AssignedTo;

            if (assignedTo != null && _contact != null)
            {
                var assignedToList = new List<AssignedToViewModel>();

                foreach (var contact in assignedTo)
                {
                    if (contact.Uuid != _contact.Uuid)
                    {
                        assignedToList.Add(new AssignedToViewModel() { Name = contact.Name, City = contact.City, Phone = contact.Phone });
                    }
                    else
                    {
                        HelpButtonViewModel.ButtonState = ButtonState.Active;
                        HelpButtonViewModel.Title = "Ich bin dabei!";
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

        private void UpdateMessage()
        {
            if (ActiveOrder!= null && ActiveOrder.Protocol == "json_v1")
            {
                _message = JsonConvert.DeserializeObject<JsonProtocolV1>(ActiveOrder.Message);

                if(_message!=null)
                {
                    Description = string.Empty;

                    if (_message.Car)
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

        private void UpdateOrderStatus()
        {
            if (ActiveOrder != null)
            {
                ActiveOrderStatus = ActiveOrder.Status;
            }
        }

        #endregion
    }
}
