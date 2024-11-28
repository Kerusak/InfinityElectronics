using InfinityElectronics.Common.Services.Interfaces;
using InfinityElectronics.Database;
using InfinityElectronics.Database.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace InfinityElectronics.ERPConnector
{
    public class ERPIntegrationFunction
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ERPIntegrationFunction> _logger;

        public ERPIntegrationFunction(HttpClient httpClient, AppDbContext dbContext, ICacheService cacheService, ILogger<ERPIntegrationFunction> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext; 
            _cacheService = cacheService;
            _logger = logger;
        }

        [Function("SyncProducts")]
        public async Task SyncProducts([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"SyncProducts function executed at: {DateTime.Now}");

            try
            {
                string url = Environment.GetEnvironmentVariable("ERPApiUrl") + "/products-sample-v1.json";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(content);

                await UpdateProductsAsync(products);

                // Assuming Redis here
                await _cacheService.SetAsync("product_list", products, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing products: {ex.Message}");
            }
        }

        [Function("SyncCategories")]
        public async Task SyncCategories([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"SyncCategories function executed at: {DateTime.Now}");

            try
            {
                string url = Environment.GetEnvironmentVariable("ERPApiUrl") + "/categories-sample-v1.json";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<Category>>(content);

                await UpdateCategoriesAsync(categories);

                // Assuming Redis here
                await _cacheService.SetAsync("categories_list", categories, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing categories: {ex.Message}");
            }
        }

        private async Task UpdateProductsAsync(List<Product> products)
        {
            foreach (var product in products)
            {
                var existingProduct = await _dbContext.Products.FindAsync(product.Id);
                if (existingProduct == null)
                {
                    _dbContext.Products.Add(product);
                }
                else
                {
                    existingProduct.Title = product.Title;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.Category = product.Category;
                    existingProduct.Image = product.Image;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateCategoriesAsync(List<Category> categories)
        {
            foreach (var category in categories)
            {
                var existingCategory = await _dbContext.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    _dbContext.Categories.Add(category);
                }
                else
                {
                    existingCategory.Name = category.Name;
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}