using EltraCloudContracts.Enka.Orders;
using EltraNotKauf.Controls;
using System.Collections.Generic;
using EnkaOrders = EltraCloudContracts.Enka.Orders;

namespace EltraNotKauf.Views.Requests
{
    public class RequestViewModel : BaseViewModel
    {
        private string _orderUuid;
        private OrderStatus _orderStatus;
        private List<OrderStatus> _orderStatusList;
        private string _orderModifiedTime;
        private string _createdBy;

        public RequestViewModel(OrderInfo orderInfo)
        {
            var orderStatusList = new List<OrderStatus>();

            orderStatusList.Add(new OrderStatus("Nicht definiert"));
            orderStatusList.Add(new OrderStatus("offen"));
            orderStatusList.Add(new OrderStatus("zugewiesen"));
            orderStatusList.Add(new OrderStatus("geschlossen"));
            orderStatusList.Add(new OrderStatus("abgelehnt"));
            orderStatusList.Add(new OrderStatus("abgelaufen"));

            OrderStatusList = orderStatusList;

            var order = orderInfo.Order;

            OrderUuid = order.Uuid;
            
            switch(order.Status)
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

            OrderModifiedTime = order.Modified.ToShortTimeString();

            var contact = orderInfo.CreatedBy;

            CreatedBy = contact.Name;
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

        public string CreatedBy 
        { 
            get => _createdBy; 
            set => SetProperty(ref _createdBy, value); 
        }
    }
}
