using Task3.Models;
using Task3.Repositories;

namespace Task3.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task CreateProductAsync(string name, decimal price)
    {
        await _productRepository.CreateProductAsync(name, price, CancellationToken.None);
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(
        int page,
        int pageSize,
        string? nameFilter = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        return await _productRepository.GetProductsAsync(page, pageSize, CancellationToken.None, nameFilter, minPrice, maxPrice);
    }
}