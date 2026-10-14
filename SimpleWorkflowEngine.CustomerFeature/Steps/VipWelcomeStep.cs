namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class VipWelcomeStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "VipWelcome";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        Console.WriteLine($"Welcome to the VIP lounge, {ctx.CustomerId}!");

        return Task.FromResult(StepResult.Success());
    }
}
