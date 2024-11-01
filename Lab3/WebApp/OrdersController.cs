using Microsoft.AspNetCore.Mvc;
using Task3.Models;
using Task3.Services;

namespace WebApp;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Создает новый заказ.
    /// </summary>
    /// <param name="request">Данные заказа.</param>
    /// <response code="200">Заказ успешно создан.</response>
    /// <response code="400">Неверные данные заказа.</response>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            return BadRequest("Неверные данные заказа.");
        }

        long orderId = await _orderService.CreateOrderAsync(request.CreatedBy);
        return Ok(orderId);
    }

    /// <summary>
    /// Добавляет товар в заказ.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <param name="request">Данные товара.</param>
    /// <response code="200">Товар успешно добавлен в заказ.</response>
    /// <response code="400">Неверные данные.</response>
    [HttpPost("{orderId}/items")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItemToOrder(long orderId, [FromBody] AddOrderItemRequest request)
    {
        if (request.ProductId <= 0 || request.Quantity <= 0)
        {
            return BadRequest("Неверные данные товара.");
        }

        await _orderService.AddItemToOrderAsync(orderId, request.ProductId, request.Quantity);
        return Ok();
    }

    /// <summary>
    /// Удаляет товар из заказа.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <param name="orderItemId">ID товара в заказе.</param>
    /// <response code="200">Товар успешно удален из заказа.</response>
    /// <response code="400">Неверные данные.</response>
    [HttpDelete("{orderId}/items/{orderItemId}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveItemFromOrder(long orderId, long orderItemId)
    {
        await _orderService.RemoveItemFromOrderAsync(orderId, orderItemId);
        return Ok();
    }

    /// <summary>
    /// Изменяет статус заказа.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <param name="request">Новый статус заказа.</param>
    /// <response code="200">Статус заказа успешно изменен.</response>
    /// <response code="400">Неверные данные.</response>
    [HttpPut("{orderId}/status")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeOrderStatus(long orderId, [FromBody] ChangeOrderStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewStatus))
        {
            return BadRequest("Неверный статус.");
        }

        await _orderService.ChangeOrderStatusAsync(orderId, request.NewStatus);
        return Ok();
    }

    /// <summary>
    /// Получает историю заказа.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <response code="200">Возвращает историю заказа.</response>
    [HttpGet("{orderId}/history")]
    [ProducesResponseType(typeof(IEnumerable<OrderHistoryItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderHistory(long orderId)
    {
        IEnumerable<OrderHistoryItem> history = await _orderService.GetOrderHistoryAsync(orderId);
        return Ok(history);
    }

    /// <summary>
    /// Получает товары в заказе.
    /// </summary>
    /// <param name="orderId">ID заказа.</param>
    /// <response code="200">Возвращает товары в заказе.</response>
    [HttpGet("{orderId}/items")]
    [ProducesResponseType(typeof(IEnumerable<OrderItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderItems(long orderId)
    {
        IEnumerable<OrderItem> items = await _orderService.GetOrderItemsAsync(orderId);
        return Ok(items);
    }
}