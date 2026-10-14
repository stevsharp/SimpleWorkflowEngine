namespace SimpleWorkflowEngine.Orders.Steps;

public class HandleDeclinedPaymentStep : IWorkflowStep<OrderContext>
{
    private readonly IEmailService _email;

    public HandleDeclinedPaymentStep(IEmailService email) => _email = email;

    public string Name => "HandleDeclinedPayment";

    public async Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        await _email.SendAsync(new EmailMessage(
            To: ctx.Order.CustomerEmail,
            Subject: $"Order {ctx.Order.Id} — Payment Failed",
            HtmlBody: "<h1>Hello</h1>"), ct);

        return StepResult.Failure("Payment was declined.");
    }
}

