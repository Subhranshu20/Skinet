

using API.Errors;
using core.Entities;
using core.Entities.OrderAggregate;
using core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private const string WhSecret="";
        private readonly IPaymentService _paymentService ;
        private readonly ILogger _logger;
        public PaymentsController(IPaymentService paymentService, ILogger logger)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket= await _paymentService.CraeteOrUpdatePaymentIntent(basketId);
            if(basket == null) return BadRequest(new ApiResponse(400,"Problem with your basket"));
            return basket;
        }
        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,Request.Headers["Stripe-Signature"],WhSecret);
           
            PaymentIntent intent;
            Order order;
            
            switch(stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent=(PaymentIntent) stripeEvent.Data.Object;
                    _logger.LogInformation("Payment Succeeded",intent.Id);
                    // TODO : Update the order with new status
                    order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                    _logger.LogInformation("Order updated to Payment received",intent.Id);
                    break;
                case "payment_intent.payment_failed":
                    intent=(PaymentIntent) stripeEvent.Data.Object;
                    _logger.LogInformation("Payment failed",intent.Id);
                    // TODO : Update the order with new status
                    order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation("Order updated to Payment failed",intent.Id);
                    break;
                
            }
            return new EmptyResult();
        }
        
    }
}