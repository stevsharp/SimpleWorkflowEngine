namespace SimpleWorkflowEngine.Orders;

public sealed record OrderItem
{
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
    public bool StockAvailable { get; init; }
    public decimal UnitPrice { get; init; }
}
