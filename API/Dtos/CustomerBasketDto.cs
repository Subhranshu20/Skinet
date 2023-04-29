using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; }
        public List<BasketItemDto> items{get;set;} 
        public int? DeliveryMethodId { get; set; }
        public string ClientSecret { get; set; }
        public string  PaymentIntentId { get; set; }
        public decimal ShippingPrice { get; set; }
    }
}