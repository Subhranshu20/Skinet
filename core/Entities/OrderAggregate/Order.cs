using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace core.Entities.OrderAggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
        }

        public Order(string buyerEmail, DeliveryMethod deliveryMethod, Address shipToAddress
        , IReadOnlyList<OrderItem> orderItems, decimal subTotal)
        {
            BuyerEmail = buyerEmail;            
            DeliveryMethod = deliveryMethod;
            ShipToAddress = shipToAddress;
            OrderItems = orderItems;
            SubTotal = subTotal; 
        }

        public string BuyerEmail { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DeliveryMethod DeliveryMethod{get;set;}
        public Address ShipToAddress { get; set; }
        public IReadOnlyList<OrderItem> OrderItems { get; set; }
        public decimal SubTotal { get; set; }
        public OrderStatus Status {get;set;} = OrderStatus.Pending;
        public string PaymentIntentId {get;set;}
        public decimal GetTotal(){
            return SubTotal + DeliveryMethod.Price;
            }
    }
}