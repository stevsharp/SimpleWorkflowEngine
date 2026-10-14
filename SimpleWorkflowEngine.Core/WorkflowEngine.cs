using System.Diagnostics;

namespace SimpleWorkflowEngine;

public sealed class WorkflowEngine<TContext> where TContext : WorkflowContext   
{
    private readonly Dictionary<string, IWorkflowStep<TContext>> _steps = new();
    private readonly Dictionary<string, string> _transitions = new();
    private readonly Dictionary<string, List<(Func<TContext, bool> Predicate, string Next)>> _routes = new();
    private string? _firstStep;

    public event Action<String>? OnStepStarted;
    public event Func<string, StepResult, long, Task>? OnStepCompleted;   

    public WorkflowEngine<TContext> AddStep(IWorkflowStep<TContext> step , string? nextStep = null)
    {
        _steps[step.Name] = step;
       _firstStep ??= step.Name;
        if (nextStep != null)
        {
            _transitions[step.Name] = nextStep;
        }

        return this;
    }

    public WorkflowEngine<TContext> AddRoute(string from, 
        Func<TContext,bool> when, string to)
    {
        if (!_routes.ContainsKey(from))
            _routes[from] = [];

        _routes[from].Add((when, to));

        return this;
    }

    public async Task<TContext> RunAsync(TContext context, CancellationToken ct = default)
    {
        context.Status = WorkflowStatus.Running;
        context.StartedAt = DateTime.UtcNow;
        context.CurrentStep = _firstStep;

        while (context.CurrentStep is not null && !ct.IsCancellationRequested)
        {
            if (!_steps.TryGetValue(context.CurrentStep, out var step))
            {
                context.Errors.Add($"Step '{context.CurrentStep}' not found.");
                context.Status = WorkflowStatus.Failed;
                break;
            }

            var sw = Stopwatch.StartNew();

            OnStepStarted?.Invoke(step.Name);

            try
            {
                var result = await step.ExecuteAsync(context, ct);
                sw.Stop();
                context.ExecutedSteps.Add(step.Name);

                if (OnStepCompleted != null)
                    await OnStepCompleted.Invoke(step.Name, result, sw.ElapsedMilliseconds);

                if (!result.IsSuccess)
                {
                    context.Errors.Add(result.ErrorMessage ?? "Unknown error");
                    context.Status = WorkflowStatus.Failed;
                    break;
                }

                context.CurrentStep = result.NextStep ??
                    ResolveNext(step.Name,context);

            }
            catch (Exception ex)
            {
                sw.Stop();
                context.Errors.Add($"Exception in '{step.Name}': {ex.Message}");
                context.Status = WorkflowStatus.Failed;
                break;
            }
        }

        if (context.Status == WorkflowStatus.Running)
        {
            context.Status = WorkflowStatus.Completed;
            context.CompletedAt = DateTime.UtcNow;
        }

        return context;
    }

    private string? ResolveNext(string currentStep, TContext context)
    {
        if (_routes.TryGetValue(currentStep, out var routes))
        {
            foreach (var (predicate, next) in routes)
            {
                if (predicate(context))
                {
                    return next;
                }
            }
        }
        _transitions.TryGetValue(currentStep, out var nextStep);

        return nextStep;
    }
}
