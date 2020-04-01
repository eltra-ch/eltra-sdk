using EltraNotKauf.Controls;
using System.Threading.Tasks;
using Xamarin.Forms;
using System;
using Newtonsoft.Json;
using EltraCloudContracts.Enka.Orders;
using System.Timers;
using EltraNotKauf.Controls.Button;
using System.ComponentModel;
using System.Windows.Input;
using EltraNotKauf.Extensions;
using EltraNotKauf.Endpoints;
using System.Collections.Generic;
using EltraNotKauf.Views.Common;
using System.Reflection;

namespace EltraNotKauf.Views.Orders
{
    public class OrderViewModel : ToolViewModel
    {
        #region Private fields

        private const double OrderUpdateInterval = 5000;

        private bool _isValid;
        private OrdersEndpoint _ordersEndpoint;
        private Order _activeOrder;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;
        private List<AssignedToViewModel> _assignedTo;

        private ThreeStateButtonViewModel _otherButtonViewModel;
        private ThreeStateButtonViewModel _shopButtonViewModel;
        private ThreeStateButtonViewModel _drugStoreButtonViewModel;
        private ThreeStateButtonViewModel _carButtonViewModel;

        private string _activeOrderStatus;
        private string _activeOrderTitle;
        private string _notice;
        private Timer _orderUpdateTimer;
        private ContactEndpoint _contactEndpoint;

        #endregion

        #region Constructors

        public OrderViewModel()
        {
            Title = "Brauche Hilfe";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.lifebuoy.png", Assembly.GetExecutingAssembly());
            IsMandatory = true;
            Uuid = "37A00C5A-3A87-40F5-B954-5BE2161728F2";

            _contactEndpoint = new ContactEndpoint();
            _ordersEndpoint = new OrdersEndpoint();
            _orderUpdateTimer = new Timer(OrderUpdateInterval);
            _orderUpdateTimer.Elapsed += OnUpdateOrderIntervalElapsed;

            OtherButtonViewModel.Id = "other;";
            OtherButtonViewModel.Title = "ANDERE";

            ShopButtonViewModel.Id = "shop;";
            ShopButtonViewModel.Title = "LEBENSMITTEL";

            CarButtonViewModel.Id = "car;";
            CarButtonViewModel.Title = "AUTO";

            DrugStoreButtonViewModel.Id = "drug_store;";
            DrugStoreButtonViewModel.Title = "APOTHEKE";

            OtherButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            ShopButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            CarButtonViewModel.ButtonStateChanged += OnButtonStateChanged;
            DrugStoreButtonViewModel.ButtonStateChanged += OnButtonStateChanged;

            PropertyChanged += OnViewPropertyChanged;
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

        public string Notice
        {
            get => _notice;
            set => SetProperty(ref _notice, value);
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

        public List<AssignedToViewModel> AssignedTo
        {
            get => _assignedTo ?? (_assignedTo = new List<AssignedToViewModel>());
            set => SetProperty(ref _assignedTo, value);
        }

        #endregion

        #region Command

        public ICommand ButtonClosePressedCommand => new Command(OnButtonClosePressedCommand);
        public ICommand ButtonCreatePressedCommand => new Command(OnButtonCreatePressedCommand);

        #endregion

        #region Events

        private async void OnButtonClosePressedCommand(object obj)
        {
            if (ActiveOrder != null)
            {
                if (ActiveOrder.Status == OrderStatus.Assigned || ActiveOrder.Status == OrderStatus.Open)
                {
                    ActiveOrder.Status = OrderStatus.Closed;

                    if (await _ordersEndpoint.ChangeOrder(ActiveOrder))
                    {
                        ActiveOrder = await _ordersEndpoint.GetActiveOrder();

                        UpdateActiveOrderStatus();
                    }
                    else
                    {
                        ActiveOrderStatus = "Fehler :-(";
                    }
                }
            }
        }

        private async void OnButtonCreatePressedCommand(object obj)
        {
            if (await _ordersEndpoint.AddOrder(new Order() { Message = CreateMessage().ToJson() }))
            {
                ActiveOrder = await _ordersEndpoint.GetActiveOrder();

                UpdateActiveOrderStatus();
            }
            else
            {
                ActiveOrderStatus = "Fehler :-(";
            }
        }

        private async void OnViewPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (IsVisible)
            {
                if (args.PropertyName == "Notice")
                {
                    await OnMessageChanged();
                }
            }
        }

        private async void OnUpdateOrderIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            var activeOrder = await _ordersEndpoint.GetActiveOrder();

            if (IsActiveOrderChanged(activeOrder))
            {
                ActiveOrder = activeOrder;

                UpdateActiveOrderStatus();
                UpdateControlsState();
                
                UpdateContactInfo();
            }
        }

        private async void OnButtonStateChanged(object sender, EventArgs e)
        {
            await OnMessageChanged();
        }

        private JsonProtocolV1 CreateMessage()
        {
            var result = new JsonProtocolV1();

            if (OtherButtonViewModel.ButtonState == ButtonState.Active)
            {
                result.Other = true;
            }

            if (ShopButtonViewModel.ButtonState == ButtonState.Active)
            {
                result.Shop = true;
            }

            if (CarButtonViewModel.ButtonState == ButtonState.Active)
            {
                result.Car = true;
            }

            if (DrugStoreButtonViewModel.ButtonState == ButtonState.Active)
            {
                result.DrugStore = true;
            }

            result.Notice = Notice;

            return result;
        }

        private async Task OnMessageChanged()
        {
            var message = CreateMessage();

            _orderUpdateTimer.Enabled = false;

            ActiveOrder = await _ordersEndpoint.GetActiveOrder();

            if (ActiveOrder != null)
            {
                if (ActiveOrder.Status == OrderStatus.Closed)
                {
                    if (await _ordersEndpoint.AddOrder(new Order() { Message = message.ToJson() }))
                    {
                        ActiveOrder = await _ordersEndpoint.GetActiveOrder();

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

                    ActiveOrder.Message = message.ToJson();

                    if (await _ordersEndpoint.ChangeOrder(ActiveOrder))
                    {
                        ActiveOrder = await _ordersEndpoint.GetActiveOrder();

                        UpdateActiveOrderStatus();
                    }
                    else
                    {
                       ActiveOrderStatus = "Fehler :-(";
                    }
                }
            }

            _orderUpdateTimer.Enabled = true;
        }

        #endregion

        #region Methods

        private async Task UpdateAssignedInfo()
        {
            var assignedToList = new List<AssignedToViewModel>();

            if (ActiveOrder != null)
            {
                var orderInfo = await _ordersEndpoint.GetOrderInfo(ActiveOrder.Uuid);
                
                var assignedTo = orderInfo?.AssignedTo;

                if (assignedTo != null && _contact != null)
                {
                    foreach (var contact in assignedTo)
                    {
                        if (contact.Uuid != _contact.Uuid)
                        {
                            assignedToList.Add(new AssignedToViewModel() { Name = contact.Name, City = contact.City, Phone = contact.Phone });
                        }                        
                    }
                }
            }

            AssignedTo = assignedToList;
        }

        private bool IsActiveOrderChanged(Order activeOrder)
        {
            bool result = false;

            if ((ActiveOrder == null) || (activeOrder != null && (activeOrder.Uuid != ActiveOrder.Uuid ||
                activeOrder.Modified != ActiveOrder.Modified)))
            {
                result = true;
            }

            return result;
        }

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

        

        public override void Show()
        {
            Task.Run(async () => {

                IsBusy = true;

                UpdateContactInfo();

                ActiveOrder = await _ordersEndpoint.GetActiveOrder();

                UpdateActiveOrderStatus();

                UpdateControlsState();

                _orderUpdateTimer.Start();

                IsBusy = false;
            });
            
            base.Show();
        }

        private void UpdateControlsState()
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

                    Notice = jsonV1.Notice;
                }
                else
                {
                    DrugStoreButtonViewModel.ButtonState = ButtonState.Inactive;
                    OtherButtonViewModel.ButtonState = ButtonState.Inactive;
                    ShopButtonViewModel.ButtonState = ButtonState.Inactive;
                    CarButtonViewModel.ButtonState = ButtonState.Inactive;

                    Notice = string.Empty;
                }
            }
            catch(Exception)
            {
            }
        }

        public override void Hide()
        {
            _orderUpdateTimer.Stop();

            base.Hide();
        }

        private void UpdateContactInfo()
        {
            var task = Task.Run(async () =>
            {
                _contact = await _contactEndpoint.GetContact();
            });

            task.ContinueWith(async (t) => { await UpdateAssignedInfo(); });
        }

        #endregion
    }
}