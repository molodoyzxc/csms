using Npgsql;
using Task3.Models;

namespace Task3.Repositories;

public class OrderItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddOrderItemAsync(long orderId, long productId, int quantity, NpgsqlTransaction transaction)
    {
        if (transaction.Connection != null)
        {
            NpgsqlCommand command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO order_items (order_id, product_id, order_item_quantity, order_item_deleted) VALUES (@OrderId, @ProductId, @Quantity, false)";
            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@Quantity", quantity);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task SoftDeleteOrderItemAsync(long orderItemId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(
                "UPDATE order_items SET order_item_deleted = true WHERE order_item_id = @OrderItemId",
                connection);
        command.Parameters.AddWithValue("@OrderItemId", orderItemId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(long orderId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            new NpgsqlCommand(
                "SELECT * FROM order_items WHERE order_id = @OrderId AND order_item_deleted = false",
                connection);
        command.Parameters.AddWithValue("@OrderId", orderId);

        var orderItems = new List<OrderItem>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var orderItem = new OrderItem
            {
                OrderItemId = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                Quantity = reader.GetInt32(3),
            };
            orderItems.Add(orderItem);
        }

        return orderItems;
    }
}