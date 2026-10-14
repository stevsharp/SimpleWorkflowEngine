namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class WelcomeStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "Welcome";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        Console.WriteLine($"Welcome, customer {ctx.CustomerId}!");

        return Task.FromResult(StepResult.Success());
    }
}
