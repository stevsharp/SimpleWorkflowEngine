using SimpleWorkflowEngine;

namespace SimpleWorkflowEngine.Tests;

internal sealed class FakeEmailService : IEmailService
{
    public List<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
