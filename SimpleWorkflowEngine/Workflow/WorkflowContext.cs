

namespace SimpleWorkflowEngine;

public enum WorkflowStatus { NotStarted, Running, Completed, Failed }

public abstract class WorkflowContext
{
    public string WorkflowId { get; } = Guid.NewGuid().ToString();
    public WorkflowStatus Status { get; set; } = WorkflowStatus.NotStarted;
    public string? CurrentStep { get; set; }
    public IList<string> ExecutedSteps { get; set; } = [];   
    public IList<string> Errors { get; set; } = [];
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WorkflowRun
{
    public string Id { get; set; } = null!;
    public string WorkflowType { get; set; } = null!;
    public string Status { get; set; } = "NotStarted";
    public string? CurrentStep { get; set; }
    public List<string> ExecutedSteps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string ContextJson { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class WorkflowStepLog
{
    public string WorkflowRunId { get; set; } = null!;
    public string StepName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}



