
using core.Entities;
using core.Entities.OrderAggregate;

namespace core.Interfaces
{
    public interface IPaymentService
    {
        Task<CustomerBasket> CraeteOrUpdatePaymentIntent(string basketId);
        Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentID);
        Task<Order> UpdateOrderPaymentFailed(string paymentIntentID);
        
    }
}