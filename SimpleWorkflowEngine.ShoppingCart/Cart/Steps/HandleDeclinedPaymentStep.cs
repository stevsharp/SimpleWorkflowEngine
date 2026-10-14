namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class HandleDeclinedPaymentStep(IEmailService email) : IWorkflowStep<CartContext>
{
    public string Name => "HandleDeclinedPayment";

    public async Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        await email.SendAsync(new EmailMessage(
            To: ctx.Cart.CustomerEmail,
            Subject: $"Order {ctx.Cart.Id} — Payment Failed",
            HtmlBody: "<h1>Your payment could not be processed</h1>"), ct);

        var reason = ctx.InsufficientFunds ? "Insufficient funds." : "Payment was declined.";
        return StepResult.Failure(reason);
    }
}
