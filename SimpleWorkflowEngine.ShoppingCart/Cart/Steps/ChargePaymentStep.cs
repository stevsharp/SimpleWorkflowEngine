namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class ChargePaymentStep : IWorkflowStep<CartContext>
{
    public string Name => "ChargePayment";

    public async Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        await Task.Delay(80, ct); // simulate payment gateway (Stripe/PayPal)

        // Simulate decline for amounts over 9999.
        ctx.PaymentDeclined = ctx.Cart.Total > 9999;

        if (!ctx.PaymentDeclined)
            ctx.PaymentId = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        return StepResult.Success();
    }
}
