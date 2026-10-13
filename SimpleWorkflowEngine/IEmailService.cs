namespace SimpleWorkflowEngine;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
