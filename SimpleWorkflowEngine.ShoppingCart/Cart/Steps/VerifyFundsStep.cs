namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class VerifyFundsStep : IWorkflowStep<CartContext>
{
    public string Name => "VerifyFunds";

    public Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        ctx.InsufficientFunds = ctx.UserAccountBalance < ctx.Cart.Total;

        return Task.FromResult(StepResult.Success());
    }
}
