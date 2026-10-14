namespace SimpleWorkflowEngine.CustomerFeature.Steps;

public sealed class FinishStep : IWorkflowStep<CustomerFeatureContext>
{
    public string Name => "Finish";

    public Task<StepResult> ExecuteAsync(CustomerFeatureContext ctx, CancellationToken ct = default)
    {
        ctx.AlreadyCompleted = true;

        Console.WriteLine("CustomerFeature finished.");

        return Task.FromResult(StepResult.Success());
    }
}
