using InfinityElectronics.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfinityElectronics.WebApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/v1/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Product ID is empty.");
            }

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // GET: api/v1/products
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and page size must be greater than zero.");
            }

            if (minPrice < 0 || maxPrice < 0)
            {
                return BadRequest("Price filters must be non-negative.");
            }

            if (minPrice > maxPrice)
            {
                return BadRequest("Minimum price cannot be greater than maximum price.");
            }

            var (products, totalCount) = await _productService.GetProductsAsync(page, pageSize, search, minPrice, maxPrice);

            return Ok(new
            {
                TotalItems = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = products
            });
        }
    }
}