using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using core.Entities;
using core.Interfaces;
using core.Specification;
using infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    
    public class ProductsController : BaseApiController
    {
        
        
        //private readonly IProductRepository _repository;
        private readonly IGenericRepository<Product> _productrepo;
        private readonly IMapper _mapper;

        public IGenericRepository<ProductBrand> _productBrandRepo { get; }
        public IGenericRepository<ProductType> _productTypeRepo { get; }

        public ProductsController(IGenericRepository<Product> productrepo
        ,IGenericRepository<ProductBrand> productBrandRepo
        ,IGenericRepository<ProductType> productTypeRepo
        ,IMapper mapper
        )
        {
            _productTypeRepo = productTypeRepo;
            _mapper = mapper;
            _productBrandRepo = productBrandRepo;
            _productrepo = productrepo; 
           
        }

        [HttpGet]
        public  async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
                [FromQuery]ProductSpecParams prodParams )
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(prodParams);

            var countSpec = new ProductWithFiltersForCounterSprecification(prodParams);
            var totalItems = await _productrepo.CountAsync(countSpec);
            var products = await _productrepo.ListAsync(spec);
            var data = _mapper
                .Map<IReadOnlyList<Product>,IReadOnlyList<ProductToReturnDto>>(products);

            return Ok(new Pagination<ProductToReturnDto>(prodParams.PageIndex,prodParams.PageSize,totalItems,data));
            // return products.Select(product=> new ProductToReturnDto{
            //     Id=product.Id,
            //     Name=product.Name,
            //     Description=product.Description,
            //     PictureUrl=product.PictureUrl,
            //     Price=product.Price,
            //     ProductBrand=product.ProductBrand.Name,
            //     ProductType=product.ProductType.Name   
            // }).ToList();
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
             var spec = new ProductsWithTypesAndBrandsSpecification(id);
             var product= await _productrepo.GetEntityWithSpec(spec);
             if(product == null)
             {
                return NotFound(new ApiResponse(404));
             }
             return _mapper.Map<Product,ProductToReturnDto>(product);
                         
        }

        [HttpGet("types")]
        public  async Task<ActionResult<List<ProductType>>> GetProductTypes()
        {
            var productTypes = await _productTypeRepo.ListAllAsync();
            return Ok(productTypes);
        }
        // [HttpGet("{types/id}")]
        // public async Task<ActionResult<ProductType>> GetProductType(int id)
        // {
        //      return await _repository.GetProductTypeByIdAsync(id);
             
        // }
        [HttpGet("brands")]
        public  async Task<ActionResult<List<ProductBrand>>> GetProductBrands()
        {
            var productTypes = await _productBrandRepo.ListAllAsync();
            return Ok(productTypes);
        }
        // [HttpGet("{brands/id}")]
        // public async Task<ActionResult<ProductBrand>> GetProductBrand(int id)
        // {
        //      return await _repository.GetProductBrandByIdAsync(id);
             
        // }
        
    }
}