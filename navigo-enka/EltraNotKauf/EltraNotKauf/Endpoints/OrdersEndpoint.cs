using EltraCloudContracts.Enka.Orders;
using EltraCommon.Helpers;
using EltraCommon.Logger;
using EltraConnector.Transport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace EltraNotKauf.Endpoints
{
    public class OrdersEndpoint
    {
        private readonly CloudTransporter _transporter;

        public OrdersEndpoint()
        {
            _transporter = new CloudTransporter();

            CountryCode = "CH";
            LanguageCode = "de";
        }

        public OrdersEndpoint(CloudTransporter transporter)
        {
            _transporter = transporter;

            CountryCode = "CH";
            LanguageCode = "de";
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

        public string CountryCode { get; set; }

        public string LanguageCode { get; set; }

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

        public async Task<OrderInfo> GetOrderInfo(string uuid)
        {
            OrderInfo result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("uuid", uuid);
                query.Add("langCode", LanguageCode);

                var url = UrlHelper.BuildUrl(Url, "/api/Orders/info", query);

                var json = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(json))
                {
                    var response = JsonConvert.DeserializeObject<OrderInfo>(json);

                    if (response != null)
                    {
                        result = response;
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetOrderInfo", e);
            }

            return result;
        }

        public async Task<List<OrderInfo>> GetOrderInfoList(string region, string city)
        {
            var result = new List<OrderInfo>();

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                query.Add("country", CountryCode);

                if (!string.IsNullOrEmpty(region))
                {
                    query.Add("region", region);
                }

                if (!string.IsNullOrEmpty(city))
                {
                    query.Add("city", city);
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

        public async Task<Order> GetActiveOrder()
        {
            Order result = null;

            try
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                var url = UrlHelper.BuildUrl(Url, "/api/orders/get", query);

                var response = await _transporter.Get(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                    foreach (var order in orders)
                    {
                        if (order.Status == OrderStatus.Open)
                        {
                            result = order;
                            break;
                        }
                        else if (order.Status == OrderStatus.Assigned)
                        {
                            result = order;
                            break;
                        }
                        else if (order.Status == OrderStatus.Closed)
                        {
                            result = order;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - GetActiveOrder", e);
            }

            return result;
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
    }
}
