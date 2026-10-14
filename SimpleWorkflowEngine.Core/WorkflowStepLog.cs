

namespace SimpleWorkflowEngine;

public class WorkflowStepLog
{
    public string WorkflowRunId { get; set; } = null!;
    public string StepName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}



