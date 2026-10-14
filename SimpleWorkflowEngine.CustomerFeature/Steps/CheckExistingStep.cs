namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class CheckExistingStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "CheckExisting";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ctx.CustomerId))
            return Task.FromResult(StepResult.Failure("CustomerId is required."));

        Console.WriteLine($"Checking customer {ctx.CustomerId} (vip={ctx.IsVip}, already={ctx.AlreadyCompleted}).");

        return Task.FromResult(StepResult.Success());
    }
}
