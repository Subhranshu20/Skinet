using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.Entities;

namespace core.Specification
{
    public class ProductWithFiltersForCounterSprecification : BaseSpecification<Product>
    {
        public ProductWithFiltersForCounterSprecification(ProductSpecParams prodParams)
        : base(x=> 
            (string.IsNullOrEmpty(prodParams.Search) || x.Name.ToLower()
            .Contains(prodParams.Search)) &&
            (!prodParams.BrandId.HasValue || x.ProductBrandId== prodParams.BrandId) &&
            (!prodParams.TypeId.HasValue || x.ProductTypeId == prodParams.TypeId)
        )
        {
                
        }
    }
}