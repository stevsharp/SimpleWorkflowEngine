namespace SimpleWorkflowEngine.ShoppingCart;

public sealed class CartContext : WorkflowContext
{
    public required Cart Cart { get; set; }
    public decimal UserAccountBalance { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool InsufficientFunds { get; set; }
    public bool PaymentDeclined { get; set; }
    public string? PaymentId { get; set; }
}
