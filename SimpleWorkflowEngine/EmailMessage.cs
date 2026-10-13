namespace SimpleWorkflowEngine;

public record EmailMessage(string To, string Subject, string HtmlBody);

public class SmtpEmailService : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
