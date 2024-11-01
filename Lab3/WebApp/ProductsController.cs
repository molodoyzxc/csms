using Microsoft.AspNetCore.Mvc;
using Task3.Models;
using Task3.Services;

namespace WebApp;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Создает новый продукт.
    /// </summary>
    /// <param name="request">Данные продукта.</param>
    /// <response code="200">Продукт успешно создан.</response>
    /// <response code="400">Неверные данные продукта.</response>
    [HttpPost]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName) || request.ProductPrice <= 0)
        {
            return BadRequest("Неверные данные продукта.");
        }

        await _productService.CreateProductAsync(request.ProductName, request.ProductPrice);
        return Ok();
    }

    /// <summary>
    /// Получает список продуктов.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="nameFilter">Фильтр по названию продукта.</param>
    /// <param name="minPrice">Минимальная цена продукта.</param>
    /// <param name="maxPrice">Максимальная цена продукта.</param>
    /// <response code="200">Возвращает список продуктов.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameFilter = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        IEnumerable<Product> products =
            await _productService.GetProductsAsync(page, pageSize, nameFilter, minPrice, maxPrice);
        return Ok(products);
    }
}