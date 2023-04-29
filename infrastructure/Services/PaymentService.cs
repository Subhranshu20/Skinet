
using core.Entities;
using core.Entities.OrderAggregate;
using core.Interfaces;
using core.Specification;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = core.Entities.Product;

namespace infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepository;
        private readonly IConfiguration _config;
        public PaymentService(IBasketRepository basketRepository,IUnitOfWork unitOfWork,IConfiguration config)
        {
            _config = config;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerBasket> CraeteOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
            var basket =await _basketRepository.GetBasketAsync(basketId);

            if(basket==null) return null;
          
            var shippingPrice =0m;
            
            if(basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod =await _unitOfWork.Repository<DeliveryMethod>()
                                        .GetByIdAsync((int)basket.DeliveryMethodId);
                shippingPrice = deliveryMethod.Price;
            }
            foreach(var item in basket.items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if(item.Price != productItem.Price)
                {
                    item.Price = productItem.Price;
                }
            }
            var service = new PaymentIntentService();
            PaymentIntent intent;
            if(string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions{
                    Amount = (long) basket.items.Sum(i=> i.Quantity * (i.Price*100)) + (long) shippingPrice *100,
                    Currency = "INR",
                    PaymentMethodTypes = new List<string>{"card"}
                };
                intent = await service.CreateAsync(options);
                basket.PaymentIntentId=intent.Id;
                basket.ClientSecret=intent.ClientSecret;
            }
            else{
                var options = new PaymentIntentUpdateOptions{
                    Amount = (long) basket.items.Sum(i=> i.Quantity * (i.Price*100)) + (long) shippingPrice *100,
                };
                await service.UpdateAsync(basket.PaymentIntentId,options);
            }
            await _basketRepository.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentID)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentID);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            if(order == null) return null;
            order.Status = OrderStatus.PaymentFailed;
            await _unitOfWork.Complete();
            return order;
            
        }

        public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentID)
        {
           var spec = new OrderByPaymentIntentIdSpecification(paymentIntentID);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            if(order == null) return null;
            order.Status = OrderStatus.PaymentReceived;
            await _unitOfWork.Complete();
            return order;
            
        }
    }
}