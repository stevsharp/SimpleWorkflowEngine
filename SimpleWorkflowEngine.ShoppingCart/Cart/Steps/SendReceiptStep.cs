namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class SendReceiptStep(IEmailService email) : IWorkflowStep<CartContext>
{
    public string Name => "SendReceipt";

    public async Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        await email.SendAsync(new EmailMessage(
            To: ctx.Cart.CustomerEmail,
            Subject: $"Order {ctx.Cart.Id} Confirmed — Payment {ctx.PaymentId}",
            HtmlBody: "<h1>Thank you for your order</h1>"), ct);

        return StepResult.Success();
    }
}
