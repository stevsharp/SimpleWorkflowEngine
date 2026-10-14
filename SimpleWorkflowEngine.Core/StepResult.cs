namespace SimpleWorkflowEngine;

public class StepResult
{
    public bool IsSuccess { get; init; }
    public string? NextStep { get; init; }
    public string? ErrorMessage { get; init; }

    public static StepResult Success() => new() { IsSuccess = true };
    public static StepResult GoTo(string step) => new() { IsSuccess = true, NextStep = step };
    public static StepResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}