namespace Task3.Models;

public abstract class OrderHistoryItem
{
    public long OrderHistoryItemId { get; set; }

    public long OrderId { get; set; }

    public DateTimeOffset OrderHistoryItemCreatedAt { get; set; }

    public string OrderHistoryItemKind { get; set; } = string.Empty;
}