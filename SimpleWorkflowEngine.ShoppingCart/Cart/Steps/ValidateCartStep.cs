namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class ValidateCartStep : IWorkflowStep<CartContext>
{
    public string Name => "ValidateCart";

    public Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        if (!ctx.Cart.Items.Any())
            return Task.FromResult(StepResult.Failure("Cart has no items."));

        if (ctx.Cart.Total <= 0)
            return Task.FromResult(StepResult.Failure("Invalid cart total."));

        if (string.IsNullOrWhiteSpace(ctx.Cart.CustomerEmail))
            return Task.FromResult(StepResult.Failure("Customer email is required."));

        return Task.FromResult(StepResult.Success());
    }
}
