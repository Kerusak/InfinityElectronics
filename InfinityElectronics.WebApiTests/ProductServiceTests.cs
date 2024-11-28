using InfinityElectronics.Common.Services.Interfaces;
using InfinityElectronics.Database;
using InfinityElectronics.Database.Models;
using InfinityElectronics.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InfinityElectronics.WebApiTests
{
    public class ProductServiceTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly AppDbContext _dbContext;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockCacheService = new Mock<ICacheService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

            _productService = new ProductService(_dbContext, _mockCacheService.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            var product1 = new Product { Id = "el-100", Title = "Test Product", Price = 100, Category = "cat-1", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            await _dbContext.SaveChangesAsync();

            // Act
            var product = await _productService.GetProductByIdAsync("el-100");

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Test Product", product.Title);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldReturnAllProducts_WhenNoFiltersApplied()
        {
            // Arrange
            var product1 = new Product { Id = "el-101", Title = "Product 1", Price = 100, Category = "cat-1", Description = "Description", Image = "image.png" };
            var product2 = new Product { Id = "el-102", Title = "Product 2", Price = 150, Category = "cat-2", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            _dbContext.Products.Add(product2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsAsync(1, 10, null, null, null);

            // Assert
            Assert.NotNull(result.Products);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Products, p => p.Title == "Product 1");
            Assert.Contains(result.Products, p => p.Title == "Product 2");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterProductsByTitle_WhenSearchIsApplied()
        {
            // Arrange
            var product1 = new Product { Id = "el-101", Title = "Test Product 1", Price = 100, Category = "cat-1", Description = "Description", Image = "image.png" };
            var product2 = new Product { Id = "el-102", Title = "Test Product 2", Price = 150, Category = "cat-1", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            _dbContext.Products.Add(product2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsAsync(1, 10, "Test Product 1", null, null);

            // Assert
            Assert.NotNull(result.Products);
            Assert.Single(result.Products); 
            Assert.Equal("Test Product 1", result.Products.First().Title);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterProductsByPrice_WhenMinAndMaxPriceAreApplied()
        {
            // Arrange
            var product1 = new Product { Id = "el-101", Title = "Cheap Product", Price = 50, Category = "cat-1", Description = "Description", Image = "image.png" };
            var product2 = new Product { Id = "el-102", Title = "Expensive Product", Price = 200, Category = "cat-1", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            _dbContext.Products.Add(product2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsAsync(1, 10, null, 100, 250);

            // Assert
            Assert.NotNull(result.Products);
            Assert.Single(result.Products); 
            Assert.Equal("Expensive Product", result.Products.First().Title); 
        }

        [Fact]
        public async Task GetProductsAsync_ShouldPaginateResults_WhenPageAndPageSizeAreApplied()
        {
            // Arrange
            var product1 = new Product { Id = "el-101", Title = "Product 1", Price = 100, Category = "cat-1", Description = "Description", Image = "image.png" };
            var product2 = new Product { Id = "el-102", Title = "Product 2", Price = 150, Category = "cat-3", Description = "Description", Image = "image.png" };
            var product3 = new Product { Id = "el-103", Title = "Product 3", Price = 200, Category = "cat-2", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            _dbContext.Products.Add(product2);
            _dbContext.Products.Add(product3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsAsync(2, 1, null, null, null); 

            // Assert
            Assert.NotNull(result.Products);
            Assert.Single(result.Products); 
            Assert.Equal("Product 2", result.Products.First().Title); 
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterProductsByMultipleFilters_WhenSearchAndPriceAreApplied()
        {
            // Arrange
            var product1 = new Product { Id = "el-101", Title = "Test Product 1", Price = 50, Category = "cat-1", Description = "Description", Image = "image.png" };
            var product2 = new Product { Id = "el-102", Title = "Test Product 2", Price = 150, Category = "cat-2", Description = "Description", Image = "image.png" };
            var product3 = new Product { Id = "el-103", Title = "Test Product 3", Price = 200, Category = "cat-2", Description = "Description", Image = "image.png" };
            _dbContext.Products.Add(product1);
            _dbContext.Products.Add(product2);
            _dbContext.Products.Add(product3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsAsync(1, 10, "Test Product", 100, 175); 

            // Assert
            Assert.NotNull(result.Products);
            Assert.Single(result.Products); 
            Assert.Equal("Test Product 2", result.Products.First().Title);
        }

    }
}
