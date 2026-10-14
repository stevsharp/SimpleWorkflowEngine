namespace SimpleWorkflowEngine.CustomerFeature;

public sealed class CustomerFeatureContext : WorkflowContext
{
    public required string CustomerId { get; set; }
    public bool AlreadyCompleted { get; set; }
    public bool IsVip { get; set; }
}
