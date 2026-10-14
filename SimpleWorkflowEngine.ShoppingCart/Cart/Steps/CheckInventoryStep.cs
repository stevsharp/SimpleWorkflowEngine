namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class CheckInventoryStep : IWorkflowStep<CartContext>
{
    public string Name => "CheckInventory";

    public Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        ctx.IsOutOfStock = ctx.Cart.Items.Any(i => !i.InStock);
        return Task.FromResult(StepResult.Success());
    }
}
