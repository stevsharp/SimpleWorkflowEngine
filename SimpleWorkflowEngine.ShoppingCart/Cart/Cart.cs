namespace SimpleWorkflowEngine.ShoppingCart;

public sealed record Cart
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required string CustomerEmail { get; init; }
    public required string CustomerName { get; init; }
    public decimal Total { get; init; }
    public IReadOnlyList<CartItem> Items { get; init; } = Array.Empty<CartItem>();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
