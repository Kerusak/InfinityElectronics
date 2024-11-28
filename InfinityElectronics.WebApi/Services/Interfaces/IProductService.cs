using InfinityElectronics.Database.Models;

namespace InfinityElectronics.WebApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(string id);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
            int page, int pageSize, string? search, decimal? minPrice, decimal? maxPrice);
    }
}
