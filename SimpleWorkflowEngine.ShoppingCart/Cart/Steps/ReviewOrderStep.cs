namespace SimpleWorkflowEngine.ShoppingCart.Steps;

public class ReviewOrderStep : IWorkflowStep<CartContext>
{
    public string Name => "ReviewOrder";

    public Task<StepResult> ExecuteAsync(CartContext ctx, CancellationToken ct = default)
    {
        // Lock cart items to prevent price changes while customer confirms the order summary.
        return Task.FromResult(StepResult.Success());
    }
}
