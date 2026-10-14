namespace SimpleWorkflowEngine.Orders.Steps;

public class CheckInventoryStep : IWorkflowStep<OrderContext>
{
    public string Name => "CheckInventory";

    public Task<StepResult> ExecuteAsync(OrderContext ctx, CancellationToken ct = default)
    {
        if (ctx?.Order?.Items == null)
        {
            return Task.FromResult(StepResult.Failure("Order or items context is missing."));
        }

        ctx.IsBackorder = ctx.Order.Items.Any(i => !i.StockAvailable);

        return Task.FromResult(StepResult.Success());
    }
}
