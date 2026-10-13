namespace SimpleWorkflowEngine;

public interface IWorkflowStep<TContext> where TContext : WorkflowContext
{
    string Name { get; }
    Task<StepResult> ExecuteAsync(TContext context, CancellationToken ct = default);
}
