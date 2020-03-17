using EltraNavigo.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraConnector.Transport;
using System;
using EltraCommon.Logger;
using Newtonsoft.Json;
using EltraCloudContracts.Enka.Orders;

namespace EltraNavigo.Views.Orders
{
    public class OrderViewModel : ToolViewModel
    {
        #region Private fields

        private bool _isValid;
        private CloudTransporter _transporter;

        #endregion

        #region Constructors

        public OrderViewModel()
        {
            Title = "Orders";
            Image = ImageSource.FromResource("EltraNavigoEnka.Resources.book-open_32px.png");
            IsMandatory = true;
            Uuid = "37A00C5A-3A87-40F5-B954-5BE2161728F2";

            _transporter = new CloudTransporter();
        }

        #endregion

        #region Properties

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
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

        public ICommand HelpCommand => new Command(OnHelpCommandClicked);
        public ICommand CoopCommand => new Command(OnCoopCommandClicked);
        public ICommand MigrosCommand => new Command(OnMigrosCommandClicked);
        public ICommand ApothekeCommand => new Command(OnApothekeCommandClicked);

        #endregion

        #region Methods

        private async void OnHelpCommandClicked(object obj)
        {
            var order = new Order() { Notice = "Help" };

            await StoreOrder(order);
        }

        private async void OnCoopCommandClicked(object obj)
        {
            var order = new Order() { Notice = "Coop" };

            await StoreOrder(order);
        }

        private async void OnMigrosCommandClicked(object obj)
        {
            var order = new Order() { Notice = "Migros" };

            await StoreOrder(order);
        }

        private async void OnApothekeCommandClicked(object obj)
        {
            var order = new Order() { Notice = "Apotheke" };

            await StoreOrder(order);
        }

        public async Task<bool> StoreOrder(Order order)
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

        #endregion
    }
}