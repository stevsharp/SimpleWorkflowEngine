namespace SimpleWorkflowEngine.Orders.Steps;

public class SendConfirmationStep(IEmailService email) : IWorkflowStep<OrderContext>
{
    private readonly IEmailService _email = email;

    public string Name => "SendConfirmation";

    public async Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        await _email.SendAsync(new EmailMessage(
            To: ctx.Order.CustomerEmail,
            Subject: $"Order {ctx.Order.Id} Confirmed — Payment {ctx.PaymentId}",
            HtmlBody: "<h1>Hello</h1>"), ct);

        return StepResult.Success();
    }
}
