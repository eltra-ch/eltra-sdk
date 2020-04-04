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
using EltraNotKauf.Controls.Toast;
using EltraNotKauf.Helpers;
using EltraNotKauf.Views.Contact;

namespace EltraNotKauf.Views.Orders
{
    [Preserve(AllMembers = true)]
    public class OrderViewModel : ToolViewModel
    {
        #region Private fields

        private const double OrderUpdateInterval = 5000;

        private bool _isValid;
        private OrdersEndpoint _ordersEndpoint;
        private Order _activeOrder;
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
        ContactViewModel _contactViewModel;
        private ICommand _createOrderCommand;

        #endregion

        #region Constructors

        public OrderViewModel(ContactViewModel contactViewModel)
        {
            Title = "Brauche Hilfe";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.lifebuoy.png", Assembly.GetExecutingAssembly());
            IsMandatory = true;
            Uuid = "37A00C5A-3A87-40F5-B954-5BE2161728F2";

            _contactViewModel = contactViewModel;

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

        [Preserve]
        public ICommand ButtonClosePressedCommand => new Command(OnButtonClosePressedCommand);
        [Helpers.Preserve]
        public ICommand ButtonCreatePressedCommand => new Command(OnButtonCreatePressedCommand);

        [Helpers.Preserve]
        public ICommand CreateOrderCommand
        {
            get => _createOrderCommand ?? (_createOrderCommand = new Command(OnButtonCreatePressedCommand));
            set => SetProperty(ref _createOrderCommand, value);
        }

        #endregion

        #region Events

        private void OnButtonClosePressedCommand()
        {
            if (ActiveOrder != null)
            {
                if (ActiveOrder.Status == OrderStatus.Assigned || ActiveOrder.Status == OrderStatus.Open)
                {
                    ActiveOrder.Status = OrderStatus.Closed;

                    Task.Run(async () => { 
                        if (await _ordersEndpoint.ChangeOrder(ActiveOrder))
                        {
                            await UpdateOrder();
                        }
                        else
                        {
                            ToastMessage.ShortAlert("Fehler :-(");
                        }
                    });
                }
            }
            else
            {
                ToastMessage.ShortAlert("Fehler :-(");
            }
        }

        private void OnButtonCreatePressedCommand()
        {
            try
            {
                Task.Run(async () =>
                {
                    if (await _ordersEndpoint.AddOrder(new Order() 
                        { 
                            Message = CreateMessage().ToJson() 
                        }))
                    {
                        await UpdateOrder();
                    }
                    else
                    {
                        ToastMessage.ShortAlert("Fehler :-(");
                    }
                });
            }
            catch(Exception)
            {
                ToastMessage.ShortAlert("Fehler :-(");
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
            await UpdateOrder();
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

            var activeOrder = await _ordersEndpoint.GetActiveOrder();

            if (activeOrder != null)
            {
                if (activeOrder.Status == OrderStatus.Closed)
                {
                    if (await _ordersEndpoint.AddOrder(new Order() { Message = message.ToJson() }))
                    {
                        activeOrder = await _ordersEndpoint.GetActiveOrder();

                        UpdateActiveOrderStatus(activeOrder);
                    }
                    else
                    {
                        ToastMessage.ShortAlert("Fehler :-(");
                    }
                }
                else
                {
                    if (activeOrder.Status == OrderStatus.Rejected)
                    {
                        activeOrder.Status = OrderStatus.Open;
                    }

                    activeOrder.Message = message.ToJson();

                    if (await _ordersEndpoint.ChangeOrder(activeOrder))
                    {
                        activeOrder = await _ordersEndpoint.GetActiveOrder();

                        UpdateActiveOrderStatus(activeOrder);
                    }
                    else
                    {
                       ToastMessage.ShortAlert("Fehler :-(");
                    }
                }
            }

            _orderUpdateTimer.Enabled = true;
        }

        #endregion

        #region Methods

        private async Task UpdateAssignedInfo(Order activeOrder)
        {
            var assignedToList = new List<AssignedToViewModel>();

            if (activeOrder != null)
            {
                var orderInfo = await _ordersEndpoint.GetOrderInfo(activeOrder.Uuid);
                var order = orderInfo?.Order;
                var assignedTo = orderInfo?.AssignedTo;

                if (order!=null && assignedTo != null && _contactViewModel != null)
                {
                    if (order.Status == OrderStatus.Open || order.Status == OrderStatus.Assigned)
                    {
                        foreach (var contact in assignedTo)
                        {
                            if (contact.Uuid != _contactViewModel.ContactUuid)
                            {
                                assignedToList.Add(new AssignedToViewModel() { Name = contact.Name, City = contact.City, Phone = contact.Phone });
                            }
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

        private void UpdateActiveOrderStatus(Order order)
        {
            if (order != null)
            {
                if (order.Status == OrderStatus.Open)
                {
                    ActiveOrderStatus = "Erstellt. Bitte warten.";
                }
                else if (order.Status == OrderStatus.Assigned)
                {
                    ActiveOrderStatus = "in Bearbeitung";
                }
                else if (order.Status == OrderStatus.Closed)
                {
                    ActiveOrderStatus = "Erledigt";
                }
                else
                {
                    ActiveOrderStatus = $"Status = {ActiveOrder.Status}";
                }

                ActiveOrderTitle = order.Uuid;
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

                await UpdateOrder();

                _orderUpdateTimer.Start();

                IsBusy = false;
            });

            base.Show();
        }

        private void UpdateControlsState(Order order)
        {
            try
            {
                if (order != null && !string.IsNullOrEmpty(order.Message))
                {
                    var jsonV1 = JsonConvert.DeserializeObject<JsonProtocolV1>(order.Message);

                    OtherButtonViewModel.ButtonState = jsonV1.Other ? ButtonState.Active : ButtonState.Inactive;
                    CarButtonViewModel.ButtonState = jsonV1.Car ? ButtonState.Active : ButtonState.Inactive;
                    ShopButtonViewModel.ButtonState = jsonV1.Shop ? ButtonState.Active : ButtonState.Inactive;
                    DrugStoreButtonViewModel.ButtonState = jsonV1.DrugStore ? ButtonState.Active : ButtonState.Inactive;

                    Notice = jsonV1.Notice;
                }
                else
                {
                    ResetControlsState();
                }
            }
            catch(Exception)
            {
            }
        }

        private void ResetControlsState()
        {
            DrugStoreButtonViewModel.ButtonState = ButtonState.Inactive;
            OtherButtonViewModel.ButtonState = ButtonState.Inactive;
            ShopButtonViewModel.ButtonState = ButtonState.Inactive;
            CarButtonViewModel.ButtonState = ButtonState.Inactive;

            Notice = string.Empty;

            ActiveOrderStatus = "noch keinen Auftrag platziert";
            ActiveOrderTitle = "";
        }

        public override void Hide()
        {
            _orderUpdateTimer.Stop();

            base.Hide();
        }

        private async Task UpdateOrder()
        {
            var activeOrder = await _ordersEndpoint.GetActiveOrder();

            if (activeOrder == null)
            {
                if (ActiveOrder != null)
                {
                    ResetControlsState();
                }

                ActiveOrder = activeOrder;
            }
            else
            {
                if (IsActiveOrderChanged(activeOrder))
                {
                    UpdateActiveOrderStatus(activeOrder);
                    UpdateControlsState(activeOrder);

                    await UpdateAssignedInfo(activeOrder);

                    ActiveOrder = activeOrder;
                }
            }
        }

        #endregion
    }
}