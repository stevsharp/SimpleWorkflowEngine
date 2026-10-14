namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class NotifyOutOfStockStep(IEmailService email) : IWorkflowStep<CartContext>
{
    public string Name => "NotifyOutOfStock";

    public async Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        await email.SendAsync(new EmailMessage(
            To: ctx.Cart.CustomerEmail,
            Subject: $"Order {ctx.Cart.Id} — Item Out of Stock",
            HtmlBody: "<h1>One or more items in your cart are out of stock</h1>"), ct);

        return StepResult.Success();
    }
}
