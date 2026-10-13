namespace SimpleWorkflowEngine.Orders.Steps;

public class NotifyBackorderStep(IEmailService email) : IWorkflowStep<OrderContext>
{
    public string Name => "NotifyBackorder";

    public async Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        await email.SendAsync(new EmailMessage(
            To: ctx.Order.CustomerEmail,
            Subject: $"Order {ctx.Order.Id} — Backorder Notice",
            HtmlBody: "<h1>Hello</h1>"), ct);

        return StepResult.Success();
    }
}

