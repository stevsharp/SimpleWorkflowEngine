namespace SimpleWorkflowEngine.Orders;

public sealed class OrderContext : WorkflowContext
{
    public required Order Order { get; set; }
    public string? PaymentId { get; set; }
    public bool IsBackorder { get; set; }
    public bool PaymentDeclined { get; set; }
}
