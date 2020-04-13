using EltraCloudContracts.Enka.Orders;
using EltraNotKauf.Controls;
using EltraNotKauf.Controls.Toast;
using EltraNotKauf.Endpoints;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using AssignerContact = EltraCloudContracts.Enka.Contacts.Contact;

namespace EltraNotKauf.Views.Common
{
    public class AssignedToViewModel : BaseViewModel
    {
        private string _name;
        private string _city;
        private string _phone;
        private Order _activeOrder;
        private AssignerContact _contact;

        public ICommand ConfirmButtonPressedCommand => new Command(OnButtonConfirmPressedCommand);

        public AssignedToViewModel(AssignerContact contact)
        {
            _contact = contact;

            Name = contact.Name;
            City = contact.City;
            Phone = contact.Phone;
        }
                
        public OrdersEndpoint OrdersEndpoint { get; set; }

        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value);   
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public Order ActiveOrder 
        { 
            get => _activeOrder; 
            set => SetProperty(ref _activeOrder, value); 
        }
        
        private void OnButtonConfirmPressedCommand()
        {
            try
            {
                Task.Run(async () =>
                {
                    if (ActiveOrder != null && _contact!=null)
                    {
                        if (await OrdersEndpoint.ConfirmOrder(ActiveOrder, _contact))
                        {
                            //await UpdateOrder();
                        }
                        else
                        {
                            ToastMessage.ShortAlert("Fehler :-(");
                        }
                    }
                });
            }
            catch (Exception)
            {
                ToastMessage.ShortAlert("Fehler :-(");
            }
        }
    }
}
