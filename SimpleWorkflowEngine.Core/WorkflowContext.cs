

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



