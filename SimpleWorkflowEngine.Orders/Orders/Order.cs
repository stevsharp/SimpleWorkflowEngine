namespace SimpleWorkflowEngine.Orders;

public sealed record Order
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required string CustomerEmail { get; init; }
    public required string CustomerName { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = Array.Empty<OrderItem>();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
