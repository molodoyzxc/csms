using Npgsql;
using System.Text.Json;
using Task3.Models;

namespace Task3.Repositories;

public class OrderHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddOrderHistoryAsync(
        long orderId,
        string kind,
        string payload,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "INSERT INTO order_history (order_id, order_history_item_kind, order_history_item_payload, order_history_item_created_at)\nVALUES (@OrderId, CAST(@Kind AS order_history_item_kind), @Payload::jsonb, NOW())",
            connection);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@Kind", kind);
        command.Parameters.AddWithValue("@Payload", payload);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderHistoryItem>> GetOrderHistoryAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var command = new NpgsqlCommand(
            "SELECT order_history_item_id, order_id, order_history_item_kind, order_history_item_payload, order_history_item_created_at FROM order_history WHERE order_id = @OrderId",
            connection);
        command.Parameters.AddWithValue("@OrderId", orderId);

        var orderHistoryList = new List<OrderHistoryItem>();
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string kind = reader.GetString(2);
            string payload = reader.GetString(3);

            OrderHistoryItem? historyItem = kind switch
            {
                "created" => JsonSerializer.Deserialize<CreatedOrderHistoryItem>(payload),
                "item_added" => JsonSerializer.Deserialize<ItemAddedOrderHistoryItem>(payload),
                "item_removed" => JsonSerializer.Deserialize<ItemRemovedOrderHistoryItem>(payload),
                "state_changed" => JsonSerializer.Deserialize<StateChangedOrderHistoryItem>(payload),
                _ => null,
            };

            if (historyItem != null)
            {
                historyItem.OrderHistoryItemId = reader.GetInt64(0);
                historyItem.OrderId = reader.GetInt64(1);
                historyItem.OrderHistoryItemKind = kind;
                historyItem.OrderHistoryItemCreatedAt = new DateTimeOffset(reader.GetDateTime(4), TimeSpan.Zero);

                orderHistoryList.Add(historyItem);
            }
        }

        return orderHistoryList;
    }
}