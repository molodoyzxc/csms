using Npgsql;
using Task3.Models;

namespace Task3.Repositories;

public class OrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateOrderAsync(string createdBy, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            "INSERT INTO orders (order_state, order_created_at, order_created_by) VALUES ('created', NOW(), @createdBy) RETURNING order_id",
            connection);
        command.Parameters.AddWithValue("@createdBy", createdBy);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null ? (long)result : throw new InvalidOperationException("Order creation failed");
    }

    public async Task UpdateOrderStatusAsync(long orderId, string newState, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(
                "UPDATE orders \nSET order_state = CAST(@NewState AS order_state) \nWHERE order_id = @OrderId",
                connection);
        command.Parameters.AddWithValue("@NewState", newState);
        command.Parameters.AddWithValue("@OrderId", orderId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string? state = null,
        string? createdBy = null)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(
                "SELECT * FROM orders\nWHERE (order_state = @State OR @State IS NULL)\nAND (order_created_by = @CreatedBy OR @CreatedBy IS NULL)\nLIMIT @PageSize OFFSET @Offset",
                connection);
        command.Parameters.AddWithValue("@State", state ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", createdBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        var orders = new List<Order>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var order = new Order(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetDateTime(2),
                reader.GetString(3));
            orders.Add(order);
        }

        return orders;
    }
}