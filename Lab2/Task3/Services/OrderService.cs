using Npgsql;
using System.Text.Json;
using Task3.Models;
using Task3.Repositories;

namespace Task3.Services;

public class OrderService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly OrderRepository _orderRepository;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly OrderHistoryRepository _orderHistoryRepository;

    public OrderService(
        NpgsqlDataSource dataSource,
        OrderRepository orderRepository,
        OrderItemRepository orderItemRepository,
        OrderHistoryRepository orderHistoryRepository)
    {
        _dataSource = dataSource;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderHistoryRepository = orderHistoryRepository;
    }

    public async Task<long> CreateOrderAsync(string createdBy)
    {
        long orderId = await _orderRepository.CreateOrderAsync(createdBy, CancellationToken.None);
        await _orderHistoryRepository.AddOrderHistoryAsync(orderId, "created", "{}", CancellationToken.None);
        return orderId;
    }

    public async Task AddItemToOrderAsync(long orderId, long productId, int quantity)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await _orderItemRepository.AddOrderItemAsync(orderId, productId, quantity, transaction);
            var payloadObject = new { product_id = productId, quantity };
            string payload = JsonSerializer.Serialize(payloadObject);
            await _orderHistoryRepository.AddOrderHistoryAsync(orderId, "item_added", payload, CancellationToken.None);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveItemFromOrderAsync(long orderId, long orderItemId)
    {
        await _orderItemRepository.SoftDeleteOrderItemAsync(orderItemId, CancellationToken.None);
        string payload = $"{{\"order_item_id\":{orderItemId}}}";
        await _orderHistoryRepository.AddOrderHistoryAsync(orderId, "item_removed", payload, CancellationToken.None);
    }

    public async Task ChangeOrderStatusAsync(long orderId, string newState)
    {
        await _orderRepository.UpdateOrderStatusAsync(orderId, newState, CancellationToken.None);
        string payload = $"{{\"new_state\":\"{newState}\"}}";
        await _orderHistoryRepository.AddOrderHistoryAsync(orderId, "state_changed", payload, CancellationToken.None);
    }

    public async Task<IEnumerable<OrderHistoryItem>> GetOrderHistoryAsync(long orderId)
    {
        return await _orderHistoryRepository.GetOrderHistoryAsync(orderId, CancellationToken.None);
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(long orderId)
    {
        return await _orderItemRepository.GetOrderItemsAsync(orderId, CancellationToken.None);
    }
}