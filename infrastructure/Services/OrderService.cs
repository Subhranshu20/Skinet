using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.Entities;
using core.Entities.OrderAggregate;
using core.Interfaces;
using core.Specification;

namespace infrastructure.Services
{
    public class OrderService : IOrderService
    {
        // private readonly IGenericRepository<Order> _orderRepo;
        // private readonly IGenericRepository<DeliveryMethod> _dmRepo;
        // public IGenericRepository<Product> _productRepo { get; }
        public IBasketRepository _basketRepo { get; }
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepo,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _basketRepo = basketRepo;
            // _productRepo = productRepo;
            // _orderRepo = orderRepo;
            // _dmRepo = dmRepo;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            // get the baskep from repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            //get items from the product repo
            var items = new List<OrderItem>();
            foreach(var item in basket.items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                var itemOrdered = new ProductItemOrdered(productItem.Id,productItem.Name
                ,productItem.PictureUrl
                );
                var orderItem = new OrderItem(itemOrdered,productItem.Price,item.Quantity);
                items.Add(orderItem);

            }
            // get delivery method from repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            // calc total
            var subtotal = items.Sum(item => item.Price * item.Quantity);
            //check to see if order exists
            var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            // create order
            
            if(order != null)
            {
                order.ShipToAddress=shippingAddress;
                order.DeliveryMethod=deliveryMethod;
                order.SubTotal=subtotal;
                _unitOfWork.Repository<Order>().Update(order);
            }
            else{
                order = new Order(buyerEmail,deliveryMethod,shippingAddress,items,subtotal,basket.PaymentIntentId);
                _unitOfWork.Repository<Order>().Add(order);
            }
            // save to db 
             
             var result = await _unitOfWork.Complete();   
             if(result <= 0) return null;
              // delete basket  
            //await _basketRepo.DeleteBasketAsync(basketId);
            // return order
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
        {
             var spec = new OrdersWithItemsAndOrderingSpecification(id,buyerEmail);
             return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);
            
            return await _unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}