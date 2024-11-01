namespace Task3.Models;

public class OrderHistory
{
    public long OrderHistoryItemId { get; set; }

    public long OrderId { get; set; }

    public string? OrderHistoryItemKind { get; set; }

    public string? OrderHistoryItemPayload { get; set; }

    public DateTimeOffset OrderHistoryItemCreatedAt { get; set; }
}