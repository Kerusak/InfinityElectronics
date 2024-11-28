using InfinityElectronics.Database.Models;
using InfinityElectronics.Database;
using InfinityElectronics.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using InfinityElectronics.Common.Services.Interfaces;

namespace InfinityElectronics.WebApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICacheService _cacheService;

        public ProductService(AppDbContext dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            return await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
            int page, int pageSize, string? search, decimal? minPrice, decimal? maxPrice)
        {
            string cacheKey = "product_list";
            var cachedProducts = await _cacheService.GetAsync<List<Product>>(cacheKey);

            if (cachedProducts != null)
            {
                var filteredProducts = cachedProducts
                    .Where(p => (string.IsNullOrEmpty(search) || p.Title.Contains(search)) &&
                                (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                                (!maxPrice.HasValue || p.Price <= maxPrice.Value))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (filteredProducts, filteredProducts.Count);
            }

            var query = _dbContext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalItems);
        }
    }
}