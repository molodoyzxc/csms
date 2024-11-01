using Npgsql;
using Task3.Models;

namespace Task3.Repositories;

public class ProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task CreateProductAsync(string name, decimal price, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command =
            new NpgsqlCommand("INSERT INTO products (product_name, product_price) VALUES (@name, @price)", connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@price", price);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string? nameFilter = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT * FROM products
                                                   WHERE (product_name ILIKE @NameFilter OR @NameFilter IS NULL)
                                                   AND (product_price::numeric >= @MinPrice OR @MinPrice IS NULL)
                                                   AND (product_price::numeric <= @MaxPrice OR @MaxPrice IS NULL)
                                                   LIMIT @PageSize OFFSET @Offset
                           """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NameFilter", nameFilter != null ? $"%{nameFilter}%" : DBNull.Value);
        command.Parameters.AddWithValue("@MinPrice", minPrice.HasValue ? minPrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@MaxPrice", maxPrice.HasValue ? maxPrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        var products = new List<Product>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var product = new Product
            {
                ProductId = reader.GetInt64(0),
                ProductName = reader.GetString(1),
                ProductPrice = reader.GetDecimal(2),
            };
            products.Add(product);
        }

        return products;
    }
}