namespace Task3.Models;

public class Order
{
    public long OrderId { get; private set; }

    public string? OrderState { get; private set; }

    public DateTime OrderCreatedAt { get; private set; }

    public string? OrderCreatedBy { get; private set; }

    public Order(long id, string orderState, DateTime createdAt, string orderCreatedBy)
    {
        OrderId = id;
        OrderState = orderState;
        OrderCreatedAt = createdAt;
        OrderCreatedBy = orderCreatedBy;
    }

    public Order(string createdBy)
    {
        OrderCreatedBy = createdBy;
        OrderState = "created";
        OrderCreatedAt = DateTime.UtcNow;
    }

    public void UpdateOrderState(string newState)
    {
        OrderState = newState;
    }
}