namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class ProvisionAccountStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "ProvisionAccount";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        Console.WriteLine($"Provisioning account for {ctx.CustomerId}.");

        return Task.FromResult(StepResult.Success());
    }
}
