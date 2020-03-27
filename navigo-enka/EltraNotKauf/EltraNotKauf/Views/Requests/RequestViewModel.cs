using EltraCloudContracts.Enka.Orders;
using EltraNotKauf.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using EnkaOrders = EltraCloudContracts.Enka.Orders;

namespace EltraNotKauf.Views.Requests
{
    public class RequestViewModel : BaseViewModel
    {
        #region Private fields

        private string _orderUuid;
        private OrderStatus _orderStatus;
        private List<OrderStatus> _orderStatusList;
        private JsonProtocolV1 _message;
        private string _orderModifiedTime;
        private string _createdBy;
        private string _orderCity;
        private string _orderPostalCode;
        private string _orderRegion;
        private string _orderStreet;
        private string _orderPhone;
        private string _description;
        private bool _requestChecked;
        private List<string> _assignedTo;

        #endregion

        #region Constructors

        public RequestViewModel(OrderInfo orderInfo)
        {
            CreateOrderStatusList();
            
            UpdateOrderInfo(orderInfo);

            UpdateContactInfo(orderInfo);

            UpdateAssignedInfo(orderInfo);
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

        #endregion

        #region Methods

        private void UpdateOrderInfo(OrderInfo orderInfo)
        {
            var order = orderInfo.Order;

            OrderUuid = order.Uuid;
            OrderModifiedTime = order.Modified.ToShortTimeString();

            UpdateMessage(order);

            UpdateOrderStatus(order);
        }

        private void UpdateAssignedInfo(OrderInfo orderInfo)
        {
            var assignedTo = orderInfo.AssignedTo;

            if (assignedTo != null)
            {
                var assignedToList = new List<string>();

                foreach (var contact in assignedTo)
                {
                    assignedToList.Add(contact.Name);
                }

                AssignedTo = assignedToList;
            }
        }

        private void UpdateContactInfo(OrderInfo orderInfo)
        {
            var contact = orderInfo.CreatedBy;

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
                        Description += ",";
                    }

                    if (_message.DrugStore)
                    {
                        Description += "APOTHEKE";
                        Description += ",";
                    }

                    if (_message.Shop)
                    {
                        Description += "LADEN";
                        Description += ",";
                    }

                    if (_message.Other)
                    {
                        Description += "ANDERE";
                        Description += ",";
                    }

                    if (!string.IsNullOrEmpty(_message.Notice))
                    {
                        Description += _message.Notice;
                    }

                    if(Description.EndsWith(","))
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
