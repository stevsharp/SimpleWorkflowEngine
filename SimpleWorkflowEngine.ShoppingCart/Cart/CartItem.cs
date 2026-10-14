namespace SimpleWorkflowEngine.ShoppingCart;

public sealed record CartItem
{
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public bool InStock { get; init; }
}
