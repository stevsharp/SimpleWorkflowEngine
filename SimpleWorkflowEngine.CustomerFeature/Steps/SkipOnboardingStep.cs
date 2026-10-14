namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class SkipOnboardingStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "SkipOnboarding";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        Console.WriteLine($"Customer {ctx.CustomerId} already onboarded — skipping.");

        return Task.FromResult(StepResult.Success());
    }
}
