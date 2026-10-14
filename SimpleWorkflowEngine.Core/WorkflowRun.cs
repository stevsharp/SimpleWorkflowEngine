

namespace SimpleWorkflowEngine;

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



