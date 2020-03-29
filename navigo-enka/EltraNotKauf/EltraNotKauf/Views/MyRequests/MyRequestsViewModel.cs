using EltraNotKauf.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.Generic;
using EltraNotKauf.Endpoints;
using EltraNotKauf.Views.Requests;

namespace EltraNotKauf.Views.MyRequests
{
    public class MyRequestsViewModel : ToolViewModel
    {
        #region Private fields

        private OrdersEndpoint _ordersEndpoint;
        private ContactEndpoint _contactEndpoint;
        private EltraCloudContracts.Enka.Contacts.Contact _contact;
        private List<RequestViewModel> _requestList;
        private bool _areRequestsVisible;

        #endregion

        #region Constructors

        public MyRequestsViewModel()
        {
            Title = "Ich bin dabei";
            Image = ImageSource.FromResource("EltraNotKauf.Resources.link.png");
            IsMandatory = true;
            Uuid = "C0A27DF7-94F8-4DF9-B2D0-E40FE7490EF2";
            
            _ordersEndpoint = new OrdersEndpoint();
            _contactEndpoint = new ContactEndpoint();            
        }

        #endregion

        #region Properties

        public bool AreRequestsVisible
        {
            get => _areRequestsVisible;
            set => SetProperty(ref _areRequestsVisible, value);
        }

        public EltraCloudContracts.Enka.Contacts.Contact Contact
        {
            get => _contact ?? (_contact = new EltraCloudContracts.Enka.Contacts.Contact());
            set => _contact = value;
        }
        
        public List<RequestViewModel> RequestList
        {
            get => _requestList ?? (_requestList = new List<RequestViewModel>());
            set => SetProperty(ref _requestList, value);
        }
        
        #endregion

        #region Command

        public ICommand RefreshCommand => new Command(OnRefreshCommand);
        
        #endregion

        #region Methods

        private async void OnRefreshCommand()
        {
            IsRefreshing = true;

            await UpdateRequests();

            IsRefreshing = false;
        }

        private async Task UpdateRequests()
        {
            {
                var orderInfoList = await _ordersEndpoint.GetAssignedToMeOrderInfoList();
                var requestList = new List<RequestViewModel>();

                foreach (var orderInfo in orderInfoList)
                {
                    var requestViewModel = new RequestViewModel(this, _ordersEndpoint, orderInfo);

                    requestViewModel.Show();

                    requestList.Add(requestViewModel);
                }

                RequestList = requestList;
                AreRequestsVisible = requestList.Count > 0;
            }
        }

        public override void Show()
        {
            Task.Run(async () =>
            {
                IsBusy = true;

                ReadContact();
                
                await UpdateRequests();

                IsBusy = false;
            });

            base.Show();
        }

        private async void ReadContact()
        {
            var contact = await _contactEndpoint.GetContact();

            if (contact != null)
            {
                Contact = contact;
            }
            else
            {
                Contact = null;
            }
        }
       
        #endregion
    }
}