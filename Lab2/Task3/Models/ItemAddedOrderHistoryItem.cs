namespace Task3.Models;

public class ItemAddedOrderHistoryItem : OrderHistoryItem
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}