
using API.Dtos;
using AutoMapper;
using core.Entities;
using core.Entities.OrderAggregate;

namespace API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product,ProductToReturnDto>()
            .ForMember(d=> d.ProductBrand , o=> o.MapFrom(s=> s.ProductBrand.Name))
            .ForMember(d=> d.ProductType , o=> o.MapFrom(s=> s.ProductType.Name))
            .ForMember(d=> d.PictureUrl, o=> o.MapFrom<ProductUrlResolver>())
            ;
            CreateMap<core.Entities.Identity.Address, AddressDto>().ReverseMap();
            CreateMap<CustomerBasketDto,CustomerBasket>();
            CreateMap<BasketItemDto,BasketItem>();
            CreateMap<AddressDto, core.Entities.OrderAggregate.Address>();
            CreateMap<Order,OrderToReturnDto>()
                .ForMember(d=> d.DeliveryMethod,o=> o.MapFrom(s=> s.DeliveryMethod.ShortName))
                .ForMember(d=> d.ShippingPrice,o=> o.MapFrom(s=> s.DeliveryMethod.Price));
                
            CreateMap<OrderItem,OrderItemDto>()
                .ForMember(d => d.ProductId,o => o.MapFrom(s=> s.ItemOrdered.ProductItemId))
                .ForMember(d => d.ProductName,o => o.MapFrom(s=> s.ItemOrdered.ProductName))
                .ForMember(d => d.PictureUrl,o => o.MapFrom(s=> s.ItemOrdered.PictureUrl))
                .ForMember(d => d.PictureUrl,o => o.MapFrom<OrderItemUrlResolver>());
        }
    }
}