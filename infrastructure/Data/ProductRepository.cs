using core.Entities;
using core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Data{
    public class ProductRepository : IProductRepository
    {
        private readonly StoreContext _context;
        public ProductRepository(StoreContext context)
        {
            _context = context;
            
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            return await _context.ProductBrands.ToListAsync();

        }

        public async Task<ProductBrand> GetProductBrandByIdAsync(int Id)
        {
           return await _context.ProductBrands.FindAsync(Id);
        }

        public async Task<Product> GetProductByIdAsync(int Id)
        {
            var prod = await _context.Products
                            .Include(p=> p.ProductType)
                            .Include(p=> p.ProductBrand)
                            .FirstOrDefaultAsync(p=> p.Id==Id);
            return prod;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await _context.Products
                            .Include(p=> p.ProductType)
                            .Include(p=> p.ProductBrand)
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            return await _context.ProductTypes
                            // .Include(p=> p.Product)
                            // .Include(p=> p.ProductBrand)
                            .ToListAsync();
        }

        public async Task<ProductType> GetProductTypeByIdAsync(int Id)
        {
            return await _context.ProductTypes.FindAsync(Id);
        }
    }
}