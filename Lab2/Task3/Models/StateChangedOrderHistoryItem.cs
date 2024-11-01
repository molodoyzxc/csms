namespace Task3.Models;

public class StateChangedOrderHistoryItem : OrderHistoryItem
{
    public string NewState { get; set; } = string.Empty;
}